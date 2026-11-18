using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Playables;

/// <summary>
/// Isolated-save opt-in presentation QA. Moves only the test player/camera for
/// the water route. Zone7 activation is explicit test setup, not a gate-success test.
/// The actual proximity Update and Animator trigger each presentation.
/// </summary>
public static class WorldPresentationRuntimeAudit
{
    const string Scope = "Isolated QA player/camera setup; actual FireflyStonePathReveal.Update/proxy animation and E Animator/feedback. Zone7 activation is setup, not totem collection or gate-success proof.";

    sealed class ColliderSnapshot
    {
        public Collider collider;
        public Matrix4x4 matrix;
        public Bounds bounds;
        public bool enabled, trigger, active;
        public ColliderSnapshot(Collider value)
        {
            collider=value; matrix=value.transform.localToWorldMatrix; bounds=value.bounds;
            enabled=value.enabled; trigger=value.isTrigger; active=value.gameObject.activeInHierarchy;
        }
        public bool Unchanged => collider && collider.transform.localToWorldMatrix==matrix && collider.bounds==bounds &&
            collider.enabled==enabled && collider.isTrigger==trigger && collider.gameObject.activeInHierarchy==active;
    }

    public static IEnumerator Run(PlayerController player, string reportPath, Action<object> record)
    {
        var args=SceneLoadingSmokeTest.GetArguments();
        if (!args.Contains("--soom-loading-smoke") || !args.Contains("--soom-world-presentation-audit"))
            throw new InvalidOperationException("World presentation requires explicit smoke/world audit flags.");
        if (!player || !player.Health || player.Health.IsDead || !Camera.main ||
            !SceneLoadingManager.Instance || !SceneLoadingManager.Instance.CurrentActiveChunk || SceneLoadingManager.Instance.CurrentActiveChunk.name!="Part1")
            throw new InvalidOperationException("World presentation needs a ready, living Part1 player and graphics camera.");
        var route=UnityEngine.Object.FindFirstObjectByType<FireflyStonePathReveal>();
        var burst=UnityEngine.Object.FindObjectsByType<EnemyIntroBurstFeedback>(FindObjectsInactive.Include,FindObjectsSortMode.None)
            .SingleOrDefault(b=>PathOf(b.transform).Contains("MonsterZone_7/"));
        if (!route || route.steps.Length!=6 || !burst || !burst.intro || !burst.feedback)
            throw new InvalidOperationException("Authored six-step route or Zone7 E burst binding missing.");
        if (route.Playing || route.Completed || burst.Played || burst.intro.Completed)
            throw new InvalidOperationException("Presentation already ran; use a fresh isolated Part1 load.");
        RequireIsolatedSavePath();
        bool freshSaveHasNoKey=!DataManager.Instance.GetGameData().GamePlayTagIdSet.Contains(FireflyStonePathReveal.CompletionSaveKey);
        if(!freshSaveHasNoKey)throw new InvalidOperationException("Fresh-game presentation save already has a completed route.");
        var stepRoots=route.steps.Select(s=>CommonParent(s.stoneRenderers.Select(r=>r.transform).ToArray())).ToArray();
        var colliders=stepRoots.SelectMany(t=>t.GetComponentsInChildren<Collider>(true)).Distinct().OrderBy(c=>c.GetInstanceID()).ToArray();
        var snapshots=colliders.Select(c=>new ColliderSnapshot(c)).ToArray();
        string initialHash=HashColliders(colliders);
        var originals=route.steps.SelectMany(s=>s.stoneRenderers).Distinct().ToArray();
        var side2=player.gameObject.scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true))
            .SingleOrDefault(t=>PathOf(t)=="Story_Part1/Side_Guide/Side_2");
        bool mascotDisabled=side2 && !side2.gameObject.activeInHierarchy;
        if (colliders.Length!=13 || !mascotDisabled)
            throw new InvalidOperationException("Expected 13 preserved route colliders and the replaced Side_2 mascot disabled.");
        string sourceData=JsonUtility.ToJson(player.Data);
        var enemy=burst.GetComponent<Enemy>();
        string enemyStat=enemy && enemy.enemyStat ? JsonUtility.ToJson(enemy.enemyStat) : null;
        string prefix=System.IO.Path.Combine(System.IO.Path.GetDirectoryName(reportPath),System.IO.Path.GetFileNameWithoutExtension(reportPath)+"-world-");
        var originalPosition=player.transform.position;
        var originalRotation=player.transform.rotation;
        var previousBlend=CinemachineCore.GetBlendOverride;
        var cameraObject=new GameObject("WorldPresentationAuditCamera");
        var focus=cameraObject.AddComponent<CinemachineCamera>();
        focus.Priority=-100;
        focus.Lens=LensSettings.FromCamera(Camera.main);
        focus.Lens.ModeOverride=LensSettings.OverrideModes.Perspective;
        focus.Lens.FieldOfView=45f;
        CinemachineCore.GetBlendOverrideDelegate blend=(from,to,definition,owner)=>to==focus || from==focus ?
            new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut,0) : previousBlend!=null ? previousBlend(from,to,definition,owner) : definition;
        var activation=new List<(GameObject obj,bool active)>();
        var playerEvents=player.Events;
        int hpBefore=player.Health.CurrentHealth, guardedAttacks=0;
        void Protect(ref PlayerDamageContext context)
        {
            guardedAttacks++;context.Data.DamageAmount=0;context.Data.StiffnessAmount=0;
            context.Data.KnockbackForce=0;context.Data.KnockbackDuration=0;context.HasSuperArmor=true;
        }
        playerEvents.BeforeDamaged+=Protect;
        bool allColliderSamplesStable=true;
        GameObject routeVolumeObject=null;
        VolumeProfile routeVolumeProfile=null;
        EIntroRiseRuntimeAudit riseObserver=null;
        FEncounterObservation fEncounter=null;
        try
        {
            // The production camera's near focus blurs this overhead QA view. Override
            // only DOF in a temporary profile; never mutate an authored volume/profile.
            var cameraData=Camera.main.GetComponent<UniversalAdditionalCameraData>();
            int volumeMask=cameraData ? cameraData.volumeLayerMask.value : 1;
            int volumeLayer=0;
            if(volumeMask!=0)while(volumeLayer<31 && (volumeMask & (1<<volumeLayer))==0)volumeLayer++;
            routeVolumeProfile=ScriptableObject.CreateInstance<VolumeProfile>();
            routeVolumeProfile.name="WorldPresentationAuditRouteProfile";
            routeVolumeProfile.Add<DepthOfField>(false).mode.Override(DepthOfFieldMode.Off);
            routeVolumeObject=new GameObject("WorldPresentationAuditRouteVolume");
            routeVolumeObject.layer=volumeLayer;
            var routeVolume=routeVolumeObject.AddComponent<Volume>();
            routeVolume.isGlobal=true;
            routeVolume.priority=float.MaxValue;
            routeVolume.weight=1f;
            routeVolume.sharedProfile=routeVolumeProfile;
            Vector3 center=route.steps.Select(s=>s.guidePosition).Aggregate(Vector3.zero,(a,b)=>a+b)/route.steps.Length-Vector3.up*1.25f;
            focus.Lens.FieldOfView=32f;
            focus.transform.position=center+new Vector3(10,34,10);
            focus.transform.LookAt(center);
            CinemachineCore.GetBlendOverride=blend; focus.Priority=2000;
            yield return null; yield return null;
            var capture=SceneLoadingSmokeTest.CaptureWorld(prefix+"route-before.png");
            while(capture.MoveNext()) {allColliderSamplesStable&=snapshots.All(s=>s.Unchanged);yield return capture.Current;}
            bool hiddenBefore=originals.All(r=>r && !r.enabled && Proxy(r));
            record?.Invoke(new {stage="world-route-before",scope=Scope,screenshot=prefix+"route-before.png",colliderCount=colliders.Length,
                colliderHash=initialHash,mascotDisabled,hiddenBefore,freshSaveHasNoKey,rendererCount=originals.Length,depths=StepDepths(route)});
            if (!hiddenBefore) throw new InvalidOperationException("Fresh route did not prepare submerged renderer-only proxies.");
            // Observe the first moving lights through the actual gameplay camera,
            // including its original post-processing. The overhead camera is only
            // re-enabled after this capture to inspect the full stone sequence.
            focus.Priority=-100;
            routeVolumeObject.SetActive(false);
            Vector3 start=GroundNear(route.startPosition,12f);
            MovePlayer(player,start,center,record,"enter-firefly-proximity");
            CinemachineCore.ResetCameraState();
            double began=Time.realtimeSinceStartupAsDouble,deadline=began+30d;
            float[] firstRise=Enumerable.Repeat(-1f,route.steps.Length).ToArray();
            bool started=false,midCaptured=false,gameCameraCaptured=false;
            double playingSince=-1d;
            while(!route.Completed && Time.realtimeSinceStartupAsDouble<deadline)
            {
                allColliderSamplesStable&=snapshots.All(s=>s.Unchanged);
                started|=route.Playing;
                if(route.Playing && playingSince<0d)playingSince=Time.realtimeSinceStartupAsDouble;
                for(int i=0;i<route.steps.Length;i++)
                    if(firstRise[i]<0 && route.steps[i].stoneRenderers.Any(r=>Proxy(r) && Depth(r)<route.submergedDepth-.02f))firstRise[i]=Time.time;
                if(!gameCameraCaptured && playingSince>=0d && Time.realtimeSinceStartupAsDouble-playingSince>=.45d)
                {
                    var visibility=MeasureGuideVisibility(route);
                    capture=SceneLoadingSmokeTest.CaptureWorld(prefix+"route-game-camera.png");
                    while(capture.MoveNext()){allColliderSamplesStable&=snapshots.All(s=>s.Unchanged);yield return capture.Current;}
                    bool actualCamera=focus.Priority.Value<0 && !routeVolumeObject.activeInHierarchy;
                    record?.Invoke(new {stage="world-route-game-camera",screenshot=prefix+"route-game-camera.png",actualCamera,
                        visibility.visibleParticles,visibility.totalParticles,visibility.maxDiameterPixels,
                        visibility.material,visibility.geometryDepthFade,visibility.emissive,visibility.cameraPosition,
                        visibility.cameraFov,visibility.startViewport,visibility.firstStepViewport,
                        scope="Actual gameplay camera/post-processing; particle viewport/size are numerical checks, screenshot visual review still required."});
                    if(!actualCamera || visibility.visibleParticles==0 || visibility.maxDiameterPixels<3f)
                        throw new InvalidOperationException("The moving firefly guide is absent or subpixel in the gameplay camera.");
                    gameCameraCaptured=true;
                    focus.Priority=2000;
                    routeVolumeObject.SetActive(true);
                }
                if(!midCaptured && firstRise.Count(t=>t>=0)>=3 && firstRise.Last()<0)
                {
                    midCaptured=true;
                    capture=SceneLoadingSmokeTest.CaptureWorld(prefix+"route-mid.png");
                    while(capture.MoveNext()){allColliderSamplesStable&=snapshots.All(s=>s.Unchanged);yield return capture.Current;}
                    record?.Invoke(new {stage="world-route-mid",screenshot=prefix+"route-mid.png",colliderHash=HashColliders(colliders),depths=StepDepths(route)});
                }
                yield return null;
            }
            yield return null; // Destroy(proxy) must be complete before checking restoration.
            allColliderSamplesStable&=snapshots.All(s=>s.Unchanged);
            capture=SceneLoadingSmokeTest.CaptureWorld(prefix+"route-after.png");
            while(capture.MoveNext()){allColliderSamplesStable&=snapshots.All(s=>s.Unchanged);yield return capture.Current;}
            bool restored=originals.All(r=>r && r.enabled && !Proxy(r));
            bool ordered=firstRise.All(t=>t>=0) && Enumerable.Range(1,firstRise.Length-1).All(i=>firstRise[i]>firstRise[i-1]);
            bool savedKey=DataManager.Instance.GetGameData().GamePlayTagIdSet.Contains(FireflyStonePathReveal.CompletionSaveKey);
            bool routePass=route.Completed && started && gameCameraCaptured && midCaptured && restored && ordered && savedKey && allColliderSamplesStable && initialHash==HashColliders(colliders);
            record?.Invoke(new {stage="world-route-complete",pass=routePass,screenshot=prefix+"route-after.png",route.Completed,started,midCaptured,
                restored,ordered,firstRise,gameCameraCaptured,savedKey,allColliderSamplesStable,colliderCount=colliders.Length,colliderHash=HashColliders(colliders),seconds=Time.realtimeSinceStartupAsDouble-began});
            if(!routePass)throw new InvalidOperationException("Water path order/collider/restoration assertion failed.");
            ReleaseRouteVolume(ref routeVolumeObject,ref routeVolumeProfile);

            // Explicit isolated presentation setup: activate the existing Zone7 hierarchy,
            // then let proximity Update/Animator perform the authored introduction.
            if(!enemy)throw new InvalidOperationException("Zone7 E owner missing.");
            int playCountBefore=burst.feedback.PlayCount;
            var feedbackCounts=burst.feedback.FeedbacksList.Select(f=>f.Timing.PlayCount).ToArray();
            for(Transform t=enemy.transform;t;t=t.parent)if(!t.gameObject.activeSelf)activation.Add((t.gameObject,false));
            Vector3 ePoint=GroundNear(enemy.transform.position+Vector3.forward*6f,5f);
            focus.Lens.FieldOfView=45f;
            var zoneF=UnityEngine.Object.FindObjectsByType<Enemy>(FindObjectsInactive.Include,FindObjectsSortMode.None)
                .Where(e=>IsF(e) && PathOf(e.transform).Contains("MonsterZone_7/")).ToArray();
            if(zoneF.Length!=1)throw new InvalidOperationException("Expected the authored single Zone7 F.");
            fEncounter=new FEncounterObservation();
            fEncounter.Attach(zoneF,"Zone7",record);
            // F lands in the first ~0.37 seconds: frame both introductions before
            // activation, then return to the established E close view after F's mid shot.
            FrameGroup(focus,zoneF.Concat(new[]{enemy}).ToArray());
            riseObserver=EIntroRiseRuntimeAudit.Observe(enemy);
            for(int i=activation.Count-1;i>=0;i--)activation[i].obj.SetActive(true);
            MovePlayer(player,ePoint,enemy.transform.position,record,"zone7-presentation-proximity-after-QA-activation");
            deadline=Time.realtimeSinceStartupAsDouble+20d;
            bool playingSeen=false,burstCaptured=false,tooEarly=false,eRiseCaptured=false,fFallCaptured=false,fLandingCaptured=false;
            float normalizedAtBurst=-1f;
            capture=SceneLoadingSmokeTest.CaptureWorld(prefix+"e-intro-before.png");
            while(capture.MoveNext())yield return capture.Current;
            while(Time.realtimeSinceStartupAsDouble<deadline)
            {
                playingSeen|=burst.intro.Playing;
                if(!fFallCaptured && fEncounter.MidpointSeen)
                {
                    fFallCaptured=true;
                    capture=SceneLoadingSmokeTest.CaptureWorld(prefix+"zone7-f-fall-mid.png");
                    while(capture.MoveNext())yield return capture.Current;
                    record?.Invoke(new {stage="world-f-fall-mid",location="Zone7",screenshot=prefix+"zone7-f-fall-mid.png",enemies=fEncounter.Snapshot()});
                }
                if(!fLandingCaptured && fEncounter.LandingObserved)
                {
                    fLandingCaptured=true;
                    capture=SceneLoadingSmokeTest.CaptureWorld(prefix+"zone7-f-ground-contact.png");
                    while(capture.MoveNext())yield return capture.Current;
                    record?.Invoke(new {stage="world-f-ground-contact",location="Zone7",screenshot=prefix+"zone7-f-ground-contact.png",enemies=fEncounter.Snapshot()});
                    focus.transform.position=enemy.transform.position+new Vector3(12,9,12);
                    focus.transform.LookAt(enemy.transform.position+Vector3.up*2.5f);
                }
                if(riseObserver.MidpointSeen && !eRiseCaptured)
                {
                    eRiseCaptured=true;
                    capture=SceneLoadingSmokeTest.CaptureWorld(prefix+"e-intro-rise-mid.png");
                    while(capture.MoveNext())yield return capture.Current;
                    record?.Invoke(new {stage="world-e-rise-mid",screenshot=prefix+"e-intro-rise-mid.png",data=riseObserver.Snapshot()});
                }
                if(enemy.animator)
                {
                    var state=enemy.animator.GetCurrentAnimatorStateInfo(0);
                    bool inIntro=state.fullPathHash==Animator.StringToHash(burst.intro.stateName);
                    if(!burst.Played && inIntro && state.normalizedTime+0.001f<burst.normalizedTriggerTime && burst.feedback.PlayCount>playCountBefore)tooEarly=true;
                    if(burst.Played && !burstCaptured)
                    {
                        burstCaptured=true;normalizedAtBurst=state.normalizedTime;
                        capture=SceneLoadingSmokeTest.CaptureWorld(prefix+"e-intro-burst.png");
                        while(capture.MoveNext())yield return capture.Current;
                        record?.Invoke(new {stage="world-e-burst",screenshot=prefix+"e-intro-burst.png",normalizedAtBurst,burst.normalizedTriggerTime,
                            inIntro,playCount=burst.feedback.PlayCount-playCountBefore,activeParticles=UnityEngine.Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None)
                                .Where(p=>p && p.particleCount>0 && Vector3.Distance(p.transform.position,enemy.transform.position)<12f).Select(p=>new{p.name,p.particleCount}).ToArray()});
                    }
                }
                if(burst.intro.Completed && burstCaptured)break;
                yield return null;
            }
            yield return null; // Observer samples after the model's LateUpdate restoration.
            riseObserver.AssertComplete(record);
            if(!eRiseCaptured)throw new InvalidOperationException("E rise midpoint screenshot was not observed.");
            float holdEnd=Time.time+.5f;
            while(Time.time<holdEnd && Time.realtimeSinceStartupAsDouble<deadline)yield return null;
            int count=burst.feedback.PlayCount-playCountBefore;
            var feedbackDeltas=burst.feedback.FeedbacksList.Select((f,i)=>new {type=f.GetType().Name,active=f.Active,
                beforeInitializationCount=feedbackCounts[i],currentCount=f.Timing.PlayCount,delta=f.Timing.PlayCount-feedbackCounts[i]}).ToArray();
            bool ePass=playingSeen && burst.intro.Completed && burst.Played && burstCaptured && count==1 && !tooEarly &&
                normalizedAtBurst+.02f>=burst.normalizedTriggerTime;
            bool sourcesUnchanged=sourceData==JsonUtility.ToJson(player.Data) && enemyStat==JsonUtility.ToJson(enemy.enemyStat);
            record?.Invoke(new {stage="world-e-complete",pass=ePass,scope=Scope,playingSeen,burst.intro.Completed,burst.Played,count,feedbackDeltas,
                tooEarly,normalizedAtBurst,burst.normalizedTriggerTime,sourcesUnchanged,playerHealth=player.Health.CurrentHealth,hpBefore,guardedAttacks,
                countScope="MMF_Player.PlayCount must increment exactly once. Per-feedback counters are diagnostic because Awake/Start can reset serialized counters during activation.",
                visualReview="Screenshots captured; image and audible sound approval remain separate from invocation/count assertions."});
            if(!ePass || !sourcesUnchanged || player.Health.CurrentHealth!=hpBefore)
                throw new InvalidOperationException("Zone7 E timing/single burst/source preservation assertion failed.");
            if(!fFallCaptured || !fLandingCaptured)throw new InvalidOperationException("Zone7 F fall/ground-contact screenshots were not captured.");
            var fCombat=ObserveFCombat(player,fEncounter,focus,prefix+"zone7-f-landed.png",record);
            while(fCombat.MoveNext())yield return fCombat.Current;
            if(SceneLoadingSmokeTest.GetArguments().Contains("--soom-heavy-counter-response-audit"))
            {
                var response=HeavyCounterResponseRuntimeAudit.Run(player,record);
                while(response.MoveNext())yield return response.Current;
            }
        }
        finally
        {
            ReleaseRouteVolume(ref routeVolumeObject,ref routeVolumeProfile);
            if(fEncounter!=null)fEncounter.Dispose();
            if(riseObserver)UnityEngine.Object.Destroy(riseObserver);
            playerEvents.BeforeDamaged-=Protect;
            foreach(var item in activation)if(item.obj)item.obj.SetActive(item.active);
            if(player){SetPlayerPosition(player,originalPosition);player.transform.rotation=originalRotation;}
            if(CinemachineCore.GetBlendOverride==blend)CinemachineCore.GetBlendOverride=previousBlend;
            if(cameraObject)UnityEngine.Object.Destroy(cameraObject);
        }
    }

    public static IEnumerator RunDungeonF(PlayerController player,string reportPath,Action<object> record)
    {
        RequireIsolatedSavePath();
        var loader=SceneLoadingManager.Instance;
        if(!player || !player.Health || player.Health.IsDead || !loader || loader.IsTeleporting ||
            !loader.CurrentActiveChunk || loader.CurrentActiveChunk.name!="Part2-B")
            throw new InvalidOperationException("Side F presentation requires a ready living Part2-B player.");
        var allF=UnityEngine.Object.FindObjectsByType<Enemy>(FindObjectsInactive.Include,FindObjectsSortMode.None)
            .Where(e=>IsF(e) && e.gameObject.scene==player.gameObject.scene).ToArray();
        record?.Invoke(new {stage="world-f-side-inventory",count=allF.Length,
            paths=allF.Select(e=>new{path=PathOf(e.transform),position=e.transform.position.ToString("F3")}).ToArray(),
            scope="All F instances in the shared Part2 scene used by A/B/C; actual entrance test is the authored B group."});
        if(allF.Length!=2)throw new InvalidOperationException("Expected exactly two F in the shared side-dungeon scene.");
        var zone=allF[0].GetComponentInParent<GroupAiZone>(true);
        if(!zone || allF.Any(e=>e.GetComponentInParent<GroupAiZone>(true)!=zone))
            throw new InvalidOperationException("Side F are not in their shared authored encounter group.");
        var director=zone.GetComponent<PlayableDirector>();
        var tagsField=typeof(GamePlayTagTrigger).GetField("_tagsToAdd",BindingFlags.Instance|BindingFlags.NonPublic);
        var trigger=UnityEngine.Object.FindObjectsByType<GamePlayTagTrigger>(FindObjectsSortMode.None)
            .SingleOrDefault(t=>t.gameObject.scene==zone.gameObject.scene && tagsField.GetValue(t) is List<GamePlayTagSO> tags &&
                tags.Any(tag=>tag && tag.ID=="SubDungeon2Enter"));
        var box=trigger ? trigger.GetComponent<BoxCollider>() : null;
        if(!director || !box || GamePlayTagManager.Instance.HasTag("SubDungeon2Enter"))
            throw new InvalidOperationException("Side F need the fresh actual SubDungeon2Enter trigger/timeline.");
        var originalPosition=player.transform.position;
        var originalRotation=player.transform.rotation;
        var playerEvents=player.Events;
        int hpBefore=player.Health.CurrentHealth,guardedAttacks=0;
        string playerSource=JsonUtility.ToJson(player.Data);
        void Protect(ref PlayerDamageContext context)
        {
            guardedAttacks++;context.Data.DamageAmount=0;context.Data.StiffnessAmount=0;
            context.Data.KnockbackForce=0;context.Data.KnockbackDuration=0;context.HasSuperArmor=true;
        }
        var previousBlend=CinemachineCore.GetBlendOverride;
        var cameraObject=new GameObject("SideFPresentationAuditCamera");
        var focus=cameraObject.AddComponent<CinemachineCamera>();
        focus.Priority=-100;focus.Lens=LensSettings.FromCamera(Camera.main);
        focus.Lens.ModeOverride=LensSettings.OverrideModes.Perspective;focus.Lens.FieldOfView=45f;
        CinemachineCore.GetBlendOverrideDelegate blend=(from,to,definition,owner)=>to==focus || from==focus ?
            new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut,0) : previousBlend!=null ? previousBlend(from,to,definition,owner) : definition;
        var encounter=new FEncounterObservation();
        string prefix=Path.Combine(Path.GetDirectoryName(reportPath),Path.GetFileNameWithoutExtension(reportPath)+"-world-side-f-");
        playerEvents.BeforeDamaged+=Protect;
        try
        {
            encounter.Attach(allF,"SideB",record);
            FrameGroup(focus,allF);CinemachineCore.GetBlendOverride=blend;focus.Priority=2000;
            var capture=SceneLoadingSmokeTest.CaptureWorld(prefix+"before.png");
            while(capture.MoveNext())yield return capture.Current;
            MovePlayer(player,GroundInsideFTrigger(box),allF[0].transform.position,record,"side-f-authored-trigger-entry");
            var controller=player.GetComponent<CharacterController>();
            if(controller && controller.enabled)controller.Move(Vector3.down*.01f);
            double deadline=Time.realtimeSinceStartupAsDouble+35d;
            bool timelineSeen=false,midCaptured=false,landingCaptured=false,entryComplete=false,combatPositionPrepared=false;
            Enemy initialExposure=null;
            while(Time.realtimeSinceStartupAsDouble<deadline)
            {
                timelineSeen|=director.state==PlayState.Playing;
                if(!midCaptured && encounter.MidpointSeen)
                {
                    midCaptured=true;capture=SceneLoadingSmokeTest.CaptureWorld(prefix+"fall-mid.png");
                    while(capture.MoveNext())yield return capture.Current;
                    record?.Invoke(new {stage="world-f-fall-mid",location="SideB",screenshot=prefix+"fall-mid.png",enemies=encounter.Snapshot()});
                }
                if(!landingCaptured && encounter.LandingObserved)
                {
                    landingCaptured=true;capture=SceneLoadingSmokeTest.CaptureWorld(prefix+"ground-contact.png");
                    while(capture.MoveNext())yield return capture.Current;
                    record?.Invoke(new {stage="world-f-ground-contact",location="SideB",screenshot=prefix+"ground-contact.png",enemies=encounter.Snapshot()});
                }
                if(encounter.CompletedObserved && !combatPositionPrepared)
                {
                    // The entrance trigger is >20m from one F's home; step into the arena immediately after the intro.
                    var first=allF.OrderByDescending(e=>Vector3.Distance(player.transform.position,FHome(e))).First();
                    MovePlayer(player,GroundNearF(player,first,allF),first.transform.position,record,"side-f-intro-complete-home-range-setup");
                    initialExposure=first;
                    combatPositionPrepared=true;
                }
                entryComplete=GamePlayTagManager.Instance.HasTag("SubDungeon2Enter") && timelineSeen && director.state!=PlayState.Playing &&
                    encounter.CompletedObserved && allF.All(e=>e._initializer!=null && e._initializer.IsInitialized("Final")) &&
                    player.InputReader.CurrentInputMode==InputReaderSO.InputMode.Gameplay;
                if(entryComplete)break;
                yield return null;
            }
            record?.Invoke(new {stage="world-f-side-entry",pass=entryComplete && midCaptured && landingCaptured,timelineSeen,entryComplete,midCaptured,landingCaptured,enemies=encounter.Snapshot()});
            if(!entryComplete || !midCaptured || !landingCaptured)throw new InvalidOperationException("Actual side F entry/fall/landing/Gameplay did not complete.");
            var combat=ObserveFCombat(player,encounter,focus,prefix+"landed.png",record,initialExposure);
            while(combat.MoveNext())yield return combat.Current;
            if(SceneLoadingSmokeTest.GetArguments().Contains("--soom-heavy-counter-response-audit"))
            {
                var response=HeavyCounterResponseRuntimeAudit.RunReverseOrder(player,record);
                while(response.MoveNext())yield return response.Current;
            }
            bool unchanged=playerSource==JsonUtility.ToJson(player.Data) && player.Health.CurrentHealth==hpBefore;
            record?.Invoke(new {stage="world-f-side-complete",pass=unchanged,hpBefore,health=player.Health.CurrentHealth,guardedAttacks,
                scope="Real SubDungeon2Enter trigger/timeline and F AI attacks against a protected QA player; player-only setup teleports, no enemy/AI/stat overrides."});
            if(!unchanged)throw new InvalidOperationException("Side F presentation changed player source stats/health.");
        }
        finally
        {
            encounter.Dispose();playerEvents.BeforeDamaged-=Protect;
            if(player){SetPlayerPosition(player,originalPosition);player.transform.rotation=originalRotation;}
            if(CinemachineCore.GetBlendOverride==blend)CinemachineCore.GetBlendOverride=previousBlend;
            if(cameraObject)UnityEngine.Object.Destroy(cameraObject);
        }
    }

    static bool IsF(Enemy enemy)=>enemy && enemy.name.IndexOf("Monster_f",StringComparison.OrdinalIgnoreCase)>=0;
    static void FrameGroup(CinemachineCamera camera,Enemy[] enemies)
    {
        Vector3 center=enemies.Select(e=>e.transform.position).Aggregate(Vector3.zero,(a,b)=>a+b)/enemies.Length;
        camera.transform.position=center+new Vector3(18,15,18);
        camera.transform.LookAt(center+Vector3.up*2.5f);
    }

    sealed class FEncounterObservation : IDisposable
    {
        sealed class Row
        {
            public Enemy enemy;
            public EnemySpawnIntro intro;
            public FIntroLandingRuntimeAudit observer;
            public EnemyStateController state;
            public Enemy_AnimationEventHandler handler;
            public string source;
            public int attacksAfterLanding,hitsAfterLanding;
            public bool attackDuringIntro;
            public Action<EnemyStateController.EnemyState,EnemyStateController.EnemyState> onState;
            public Action onHit;
        }
        readonly List<Row> _rows=new List<Row>();
        string _location;
        public Enemy[] Enemies=>_rows.Select(row=>row.enemy).ToArray();
        public bool MidpointSeen=>_rows.Count>0 && _rows.All(row=>row.observer.MidpointSeen);
        public bool LandingObserved=>_rows.Count>0 && _rows.All(row=>row.observer.LandingObserved);
        public bool CompletedObserved=>_rows.Count>0 && _rows.All(row=>row.observer.CompletedObserved);
        public bool HasAttacked(Enemy enemy)=>_rows.Any(row=>row.enemy==enemy && (row.attacksAfterLanding>0 || row.hitsAfterLanding>0));
        public void Attach(Enemy[] enemies,string location,Action<object> record)
        {
            _location=location;
            foreach(var enemy in enemies)
            {
                var row=new Row{enemy=enemy,intro=enemy.GetComponent<EnemySpawnIntro>(),state=enemy.GetComponent<EnemyStateController>(),
                    handler=enemy.GetComponent<Enemy_AnimationEventHandler>(),source=JsonUtility.ToJson(enemy.enemyStat)};
                _rows.Add(row);
                if(enemy.GetComponents<EnemySpawnIntro>().Length>1 || !row.state || !row.handler || (row.intro && (row.intro.Completed || row.intro.Playing)))
                    throw new InvalidOperationException("Fresh F intro/state/handler unavailable: "+PathOf(enemy.transform));
                row.observer=FIntroLandingRuntimeAudit.Observe(enemy);
                row.onState=(previous,next)=>
                {
                    if(next!=EnemyStateController.EnemyState.Attack)return;
                    var intro=ResolveIntro(row);
                    if(intro && intro.Completed)row.attacksAfterLanding++;
                    else if(intro && intro.Playing)row.attackDuringIntro=true;
                };
                row.onHit=()=>{var intro=ResolveIntro(row);if(intro && intro.Completed)row.hitsAfterLanding++;else if(intro && intro.Playing)row.attackDuringIntro=true;};
                row.state.OnStateChanged+=row.onState;row.handler.HitWindowOpened+=row.onHit;
            }
            record?.Invoke(new {stage="world-f-observe",location,count=enemies.Length,paths=enemies.Select(e=>PathOf(e.transform)).ToArray()});
        }
        static EnemySpawnIntro ResolveIntro(Row row)
        {
            if(!row.intro && row.enemy)row.intro=row.enemy.GetComponent<EnemySpawnIntro>();
            return row.intro;
        }
        public object[] Snapshot()=>_rows.Select(row=>
        {
            var intro=ResolveIntro(row);
            var target=row.enemy.player;
            var board=row.enemy._aiController!=null && row.enemy._aiController._aiBrain!=null ? row.enemy._aiController._aiBrain.blackboard : null;
            Vector3 home=FHome(row.enemy);
            return (object)new {path=PathOf(row.enemy.transform),location=_location,instanceId=row.enemy.GetInstanceID(),
                position=row.enemy.transform.position.ToString("F3"),home=home.ToString("F3"),
                distanceFromHome=Vector3.Distance(row.enemy.transform.position,home),
                playerPosition=target ? target.transform.position.ToString("F3") : null,
                playerDistance=target ? Vector3.Distance(row.enemy.transform.position,target.transform.position) : -1f,
                playerForwardAngle=target ? Vector3.Angle(row.enemy.transform.forward,target.transform.position-row.enemy.transform.position) : -1f,
                detectRange=row.enemy.enemyStat.DetectRange,
                returningHome=board!=null && board.GetValueOrDefault<bool>("GoHome",false),
                attackToken=board!=null && board.GetValueOrDefault<bool>("HasAttackToken",false),
                pathDestination=row.enemy.aIPath ? row.enemy.aIPath.destination.ToString("F3") : null,
                lastAttackSuccessAge=board!=null ? Time.time-board.GetValueOrDefault<float>(EnemyBlackboardKeys.LastAttackSuccessTime,Time.time) : -1f,
                lastTakeHitAge=board!=null ? Time.time-board.GetValueOrDefault<float>(EnemyBlackboardKeys.LastTakeHitTime,Time.time) : -1f,
                introPlaying=intro && intro.Playing,introCompleted=intro && intro.Completed,row.attacksAfterLanding,row.hitsAfterLanding,row.attackDuringIntro,
                state=row.state.CurrentState.ToString(),stateLocked=row.state.IsStateLocked,
                combat=row.enemy._aiController && row.enemy._aiController._aiBrain!=null && row.enemy._aiController._aiBrain._isCombat,
                landing=row.observer ? row.observer.Snapshot() : null};
        }).ToArray();
        public void AssertComplete(Action<object> record)
        {
            foreach(var row in _rows)row.observer.AssertComplete(record);
            bool pass=_rows.All(row=>ResolveIntro(row) && row.enemy.GetComponents<EnemySpawnIntro>().Length==1 && row.intro.Completed && !row.intro.Playing && !row.attackDuringIntro && HasAttacked(row.enemy) &&
                row.enemy._aiController && row.enemy._aiController._aiBrain!=null && row.enemy._aiController._aiBrain._isCombat &&
                row.source==JsonUtility.ToJson(row.enemy.enemyStat));
            record?.Invoke(new {stage="world-f-landing-to-combat",location=_location,pass,enemies=Snapshot(),
                scope="Actual landing geometry and subsequent AI Attack state/hit window; protected QA player, no received-damage claim."});
            if(!pass)throw new InvalidOperationException("F landing -> actual combat assertion failed in "+_location);
        }
        public void Dispose()
        {
            foreach(var row in _rows)
            {
                if(row.state && row.onState!=null)row.state.OnStateChanged-=row.onState;
                if(row.handler && row.onHit!=null)row.handler.HitWindowOpened-=row.onHit;
                if(row.observer)UnityEngine.Object.Destroy(row.observer);
            }
        }
    }

    static IEnumerator ObserveFCombat(PlayerController player,FEncounterObservation encounter,CinemachineCamera focus,string screenshot,Action<object> record,Enemy initialExposure=null)
    {
        FrameGroup(focus,encounter.Enemies);
        double deadline=Time.realtimeSinceStartupAsDouble+25d,nextRangeCheck=0d;
        Enemy exposureTarget=initialExposure;
        while(Time.realtimeSinceStartupAsDouble<deadline)
        {
            if(encounter.CompletedObserved && encounter.Enemies.All(encounter.HasAttacked))break;
            if(encounter.CompletedObserved)
            {
                var pending=encounter.Enemies.Where(enemy=>!encounter.HasAttacked(enemy))
                    .OrderByDescending(enemy=>Vector3.Distance(player.transform.position,FHome(enemy))).FirstOrDefault();
                bool changed=pending && (exposureTarget==null || encounter.HasAttacked(exposureTarget));
                if(!changed && exposureTarget && !encounter.HasAttacked(exposureTarget))pending=exposureTarget;
                bool needsReacquisition=pending && pending._aiController!=null && pending._aiController._aiBrain!=null &&
                    !pending._aiController._aiBrain._isCombat && Time.realtimeSinceStartupAsDouble>=nextRangeCheck;
                if(pending && (changed || needsReacquisition))
                {
                    MovePlayer(player,GroundNearF(player,pending,encounter.Enemies),pending.transform.position,record,"post-landing-F-QA-player-only-range-setup");
                    exposureTarget=pending;nextRangeCheck=Time.realtimeSinceStartupAsDouble+5d;
                    record?.Invoke(new {stage="world-f-combat-range",targetId=pending.GetInstanceID(),needsReacquisition,enemies=encounter.Snapshot()});
                }
                // Hold the observation point while combat is active; chasing a retreating F every 5s can push it past its leash.
            }
            yield return null;
        }
        yield return null;
        var capture=SceneLoadingSmokeTest.CaptureWorld(screenshot);
        while(capture.MoveNext())yield return capture.Current;
        record?.Invoke(new {stage="world-f-landed-view",screenshot,enemies=encounter.Snapshot()});
        encounter.AssertComplete(record);
    }

    static Vector3 GroundInsideFTrigger(BoxCollider collider)
    {
        foreach(float z in new[]{0f,-.3f,.3f})foreach(float x in new[]{0f,-.3f,.3f})
        {
            Vector3 desired=collider.transform.TransformPoint(collider.center+new Vector3(collider.size.x*x,0,collider.size.z*z));
            if(TryFGround(desired,out var point) && (collider.ClosestPoint(point+Vector3.up*.8f)-(point+Vector3.up*.8f)).sqrMagnitude<.0001f)return point;
        }
        throw new InvalidOperationException("No safe floor inside authored F entry trigger.");
    }
    static Vector3 FHome(Enemy enemy)=>enemy._aiController!=null && enemy._aiController._aiBrain!=null ?
        enemy._aiController._aiBrain.blackboard.GetValueOrDefault<Vector3>(EnemyBlackboardKeys.HomePosition,enemy.StartPos) : enemy.StartPos;
    static float ControllerRadius(Transform owner)
    {
        var controller=owner.GetComponent<CharacterController>();
        return controller ? controller.radius*Mathf.Max(Mathf.Abs(owner.lossyScale.x),Mathf.Abs(owner.lossyScale.z)) : .5f;
    }
    static Vector3 GroundNearF(PlayerController player,Enemy enemy,Enemy[] enemies)
    {
        var zone=enemy.GetComponentInParent<GroupAiZone>(true);
        Vector3 forward=enemy.transform.forward;forward.y=0;forward=forward.sqrMagnitude>.01f ? forward.normalized : Vector3.forward;
        float maximumRange=Mathf.Min(4.5f,enemy.enemyStat.DetectRange-.25f);
        float playerRadius=ControllerRadius(player.transform);
        bool found=false;Vector3 best=default;float bestScore=float.PositiveInfinity;
        foreach(float radius in new[]{maximumRange,maximumRange-.5f,maximumRange-1f})
        foreach(float angle in new[]{0f,-15f,15f,-30f,30f,-40f,40f})
        {
            if(radius<3.5f)continue;
            Vector3 desired=enemy.transform.position+Quaternion.AngleAxis(angle,Vector3.up)*forward*radius;
            if(!TryFGround(desired,out var point) || !zone || !zone.IsInZone(point) || Mathf.Abs(point.y-enemy.transform.position.y)>2f ||
                Vector3.Distance(point,enemy.transform.position)>=enemy.enemyStat.DetectRange ||
                enemies.Any(other=>Vector3.ProjectOnPlane(other.transform.position-point,Vector3.up).magnitude<ControllerRadius(other.transform)+playerRadius+.35f))continue;
            if(Physics.Linecast(point+Vector3.up,enemy.transform.position+Vector3.up,LayerMask.GetMask("Wall","Default"),QueryTriggerInteraction.Ignore))continue;
            float ownHomeDistance=Vector3.ProjectOnPlane(point-FHome(enemy),Vector3.up).magnitude;
            float furthestHome=enemies.Max(other=>Vector3.ProjectOnPlane(point-FHome(other),Vector3.up).magnitude);
            float score=ownHomeDistance*2f+furthestHome+Mathf.Max(0f,furthestHome-13f)*10f+Mathf.Abs(angle)*.002f;
            if(score<bestScore){found=true;best=point;bestScore=score;}
        }
        if(found)return best;
        throw new InvalidOperationException("No safe in-zone forward F observation point inside its actual detection range.");
    }
    static bool TryFGround(Vector3 desired,out Vector3 point)
    {
        point=default;
        if(!Physics.Raycast(desired+Vector3.up*10f,Vector3.down,out var hit,24f,LayerMask.GetMask("Ground"),QueryTriggerInteraction.Ignore) || hit.normal.y<.75f)return false;
        point=hit.point+Vector3.up*.12f;
        return !Physics.CheckCapsule(point+Vector3.up*.4f,point+Vector3.up*1.2f,.4f,LayerMask.GetMask("Wall","Default"),QueryTriggerInteraction.Ignore);
    }

    // Called after the real SavePoint.Interact reload and again after the dungeon
    // portal round trip, with all preceding camera/guard scopes disposed.
    public static void VerifySavedReload(Action<object> record)
    {
        string savePath=RequireIsolatedSavePath();
        var loader=SceneLoadingManager.Instance;
        if(!loader || loader.IsTeleporting || !loader.CurrentActiveChunk || loader.CurrentActiveChunk.name!="Part1" || loader.LastLoadError!=null)
            throw new InvalidOperationException("Route persistence verification requires the completed Part1 save/reload.");
        var route=UnityEngine.Object.FindFirstObjectByType<FireflyStonePathReveal>();
        if(!route)throw new InvalidOperationException("Reloaded Part1 is missing the firefly route.");
        var originals=route.steps.SelectMany(step=>step.stoneRenderers).Distinct().ToArray();
        bool visible=originals.All(r=>r && r.enabled && r.gameObject.activeInHierarchy);
        bool noProxies=originals.All(r=>r && !Proxy(r));
        bool noGuide=route.GetComponentsInChildren<ParticleSystem>(true).Length==0;
        bool memoryKey=DataManager.Instance.GetGameData().GamePlayTagIdSet.Contains(FireflyStonePathReveal.CompletionSaveKey);
        int slot=(int)typeof(DataManager).GetField("_currentSlotIndex",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(DataManager.Instance);
        var disk=JsonUtility.FromJson<SaveDataContainer>(File.ReadAllText(savePath));
        bool diskKey=disk!=null && disk.DataList!=null && slot>=0 && slot<disk.DataList.Count &&
            disk.DataList[slot].GamePlayTagIdSet.Contains(FireflyStonePathReveal.CompletionSaveKey);
        bool pass=route.Completed && !route.Playing && visible && noProxies && noGuide && memoryKey && diskKey;
        record?.Invoke(new {stage="world-route-saved-reload",pass,slot,savePath,route.Completed,route.Playing,visible,noProxies,noGuide,memoryKey,diskKey,
            scope="Saved path after real save/portal reload, plus current slot JSON readback. No user save file is read or modified."});
        if(!pass)throw new InvalidOperationException("The saved stone path hid or replayed after the real SavePoint reload.");
    }

    static string RequireIsolatedSavePath()
    {
        var args=SceneLoadingSmokeTest.GetArguments();
        if(!args.Contains("--soom-loading-smoke") || !args.Contains("--soom-world-presentation-audit") || !DataManager.Instance)
            throw new InvalidOperationException("Route QA requires explicit isolated world audit flags.");
        string save=(string)typeof(DataManager).GetField("_saveFileName",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(DataManager.Instance);
        string userDirectory=Path.GetFullPath(Application.persistentDataPath).TrimEnd(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar);
        if(string.IsNullOrEmpty(save) || !Path.IsPathRooted(save) || !save.EndsWith(".smoke-save.json",StringComparison.OrdinalIgnoreCase) ||
            Path.GetFullPath(save).StartsWith(userDirectory+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Refusing world QA without an isolated smoke save.");
        return save;
    }

    sealed class GuideVisibility
    {
        public int visibleParticles,totalParticles;
        public float maxDiameterPixels,geometryDepthFade,emissive,cameraFov;
        public string material,cameraPosition,startViewport,firstStepViewport;
    }

    static GuideVisibility MeasureGuideVisibility(FireflyStonePathReveal route)
    {
        var camera=Camera.main;
        var result=new GuideVisibility {cameraPosition=camera.transform.position.ToString("F3"),cameraFov=camera.fieldOfView,
            startViewport=camera.WorldToViewportPoint(route.startPosition).ToString("F3"),firstStepViewport=camera.WorldToViewportPoint(route.steps[0].guidePosition).ToString("F3")};
        float captureHeight=Mathf.RoundToInt(1280f/camera.aspect);
        foreach(var system in route.GetComponentsInChildren<ParticleSystem>(true))
        {
            var renderer=system.GetComponent<ParticleSystemRenderer>();
            if(!renderer || !renderer.enabled || (camera.cullingMask & (1<<renderer.gameObject.layer))==0)continue;
            var material=renderer.sharedMaterial;
            if(material)
            {
                result.material=material.name;
                if(material.HasProperty("_GeometryDepthFadeDistance"))result.geometryDepthFade=material.GetFloat("_GeometryDepthFadeDistance");
                if(material.HasProperty("_EmissiveIntensity"))result.emissive=material.GetFloat("_EmissiveIntensity");
            }
            var particles=new ParticleSystem.Particle[Mathf.Max(1,system.particleCount)];
            int count=system.GetParticles(particles);result.totalParticles+=count;
            for(int i=0;i<count;i++)
            {
                var particle=particles[i];
                var position=system.main.simulationSpace==ParticleSystemSimulationSpace.World ? particle.position : system.transform.TransformPoint(particle.position);
                var viewport=camera.WorldToViewportPoint(position);
                if(viewport.z<=camera.nearClipPlane || viewport.x<0 || viewport.x>1 || viewport.y<0 || viewport.y>1 || particle.GetCurrentColor(system).a<16)continue;
                result.visibleParticles++;
                float diameter=particle.GetCurrentSize(system);
                var upper=camera.WorldToViewportPoint(position+camera.transform.up*diameter);
                result.maxDiameterPixels=Mathf.Max(result.maxDiameterPixels,Mathf.Abs(upper.y-viewport.y)*captureHeight);
            }
        }
        return result;
    }

    static void ReleaseRouteVolume(ref GameObject volumeObject,ref VolumeProfile profile)
    {
        if(volumeObject)
        {
            volumeObject.SetActive(false); // Unregister before the next E camera render.
            var volume=volumeObject.GetComponent<Volume>();
            if(volume)volume.sharedProfile=null;
            UnityEngine.Object.Destroy(volumeObject);
        }
        volumeObject=null;
        if(profile)
        {
            foreach(var component in profile.components)if(component)UnityEngine.Object.Destroy(component);
            UnityEngine.Object.Destroy(profile);
        }
        profile=null;
    }

    static Transform CommonParent(Transform[] transforms)
    {
        if(transforms.Length==0)throw new InvalidOperationException("Empty route renderer group.");
        for(var candidate=transforms[0];candidate;candidate=candidate.parent)
            if(transforms.All(t=>t==candidate || t.IsChildOf(candidate)))return candidate;
        throw new InvalidOperationException("Route group has no common transform.");
    }
    static Transform Proxy(MeshRenderer renderer)=>renderer ? renderer.transform.Find(renderer.name+"_RiseVisual") : null;
    static float Depth(MeshRenderer renderer){var proxy=Proxy(renderer);return proxy?renderer.transform.position.y-proxy.position.y:0f;}
    static object[] StepDepths(FireflyStonePathReveal route)=>route.steps.Select((step,index)=>(object)new {index,depths=step.stoneRenderers.Select(Depth).ToArray()}).ToArray();
    static string PathOf(Transform transform)=>transform.parent?PathOf(transform.parent)+"/"+transform.name:transform.name;
    static string HashColliders(Collider[] colliders)
    {
        using(var stream=new MemoryStream())using(var writer=new BinaryWriter(stream))
        {
            foreach(var c in colliders)
            {
                writer.Write(c.GetInstanceID());writer.Write(c.enabled);writer.Write(c.isTrigger);writer.Write(c.gameObject.activeInHierarchy);
                var matrix=c.transform.localToWorldMatrix;for(int i=0;i<16;i++)writer.Write(matrix[i]);
                var bounds=c.bounds;for(int i=0;i<3;i++){writer.Write(bounds.center[i]);writer.Write(bounds.size[i]);}
            }
            writer.Flush();using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(stream.ToArray())).Replace("-","").ToLowerInvariant();
        }
    }
    static Vector3 GroundNear(Vector3 desired,float radius)
    {
        foreach(float distance in new[]{0f,1f,2f,4f})for(int i=0;i<(distance==0?1:16);i++)
        {
            Vector3 point=desired+Quaternion.AngleAxis(i*22.5f,Vector3.up)*Vector3.forward*Mathf.Min(radius,distance);
            if(!Physics.Raycast(point+Vector3.up*12,Vector3.down,out var hit,30,LayerMask.GetMask("Ground"),QueryTriggerInteraction.Ignore) || hit.normal.y<.75f)continue;
            point=hit.point+Vector3.up*.12f;
            if(!Physics.CheckCapsule(point+Vector3.up*.4f,point+Vector3.up*1.2f,.4f,LayerMask.GetMask("Wall","Default"),QueryTriggerInteraction.Ignore))return point;
        }
        throw new InvalidOperationException("No safe floor near requested presentation setup.");
    }
    static void SetPlayerPosition(PlayerController player,Vector3 point)
    {
        var controller=player.GetComponent<CharacterController>();bool enabled=controller && controller.enabled;
        try{if(controller)controller.enabled=false;player.transform.position=point;}
        finally{if(controller)controller.enabled=enabled;}
        Physics.SyncTransforms();
    }
    static void MovePlayer(PlayerController player,Vector3 point,Vector3 face,Action<object> record,string reason)
    {
        var before=player.transform.position;SetPlayerPosition(player,point);
        Vector3 direction=face-point;direction.y=0;if(direction.sqrMagnitude>.01f)player.transform.rotation=Quaternion.LookRotation(direction);
        record?.Invoke(new {stage="world-presentation-setup",reason,from=before.ToString("F3"),to=point.ToString("F3"),scope=Scope});
    }
}
