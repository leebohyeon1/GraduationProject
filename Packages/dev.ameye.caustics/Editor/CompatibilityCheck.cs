using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

namespace Caustics.Editor
{
    public class CompatibilityCheck : EditorWindow
    {
        private List<Check> checks;

        private enum CheckStatus
        {
            Untested,
            InProgress,
            Completed
        }

        private enum ResultEnum
        {
            Untested,
            Pass,
            Fail,
            Warning
        }

        private class CheckResult
        {
            public ResultEnum Result { get; }
            public string Message { get; }

            public string Description { get; }

            public CheckResult(ResultEnum result, string message, string description)
            {
                Result = result;
                Message = message;
                Description = description;
            }

            public override string ToString()
            {
                return Message;
            }
        }
        
        private static SearchRequest _urpSearchRequest;
        private static AddRequest _addRequest;
        
        private const string NoActiveRendererFoundDescription
            = "Not found. See Edit > Project Settings > Graphics if there is a Default Render Pipeline assigned.";

        [MenuItem("Window/Water Caustics for URP/Compatibility")]
        public static void ShowWindow()
        {
            var window = GetWindow<CompatibilityCheck>();
            window.titleContent = new GUIContent("Water Caustics for URP", EditorGUIUtility.IconContent("Settings").image);
            window.minSize = new Vector2(400, 400);
            window.maxSize = new Vector2(400, 400);
        }

        private void CreateGUI()
        {
            checks = new List<Check>
            {
                new("Unity Version", CheckUnityVersion),
                new("URP Version", CheckURPVersion),
                new("Active Renderer", CheckActiveRenderer),
                new("Render Graph", CheckRenderGraph),
                new("Rendering Path", CheckRenderingPath),
                new("Graphics API", CheckGraphicsAPI),
                new("Platform", CheckTargetPlatform),
                new("Depth Texture", CheckDepthTexture),
                new("Opqaue Texture", CheckOpaqueTexture),
                new("Caustics Pass", CheckCausticsPass)
            };

            var container = new VisualElement
            {
                style =
                {
                    flexGrow = 1
                }
            };

            var detailsHeader = new VisualElement
            {
                style =
                {
                    paddingTop = 10,
                    paddingLeft = 5
                }
            };

            // Title.
            var titleContainer = new VisualElement
            {
                style =
                {
                    marginLeft = 4,
                    marginRight = 4,
                    paddingLeft = 2
                }
            };
            var titleHeader = new Label("Water Caustics for URP")
            {
                style =
                {
                    fontSize = 19,
                    minWidth = 100,
                    marginTop = 0,
                    paddingBottom = 2,
                    paddingLeft = 0,
                    paddingRight = 2,
                    paddingTop = 1,
                    unityFontStyleAndWeight = FontStyle.Bold
                }
            };
            titleContainer.Add(titleHeader);
            detailsHeader.Add(titleContainer);

            // Version.
            var versionContainer = new VisualElement
            {
                style =
                {
                    marginLeft = 2,
                    marginTop = 4,
                    paddingLeft = 2
                }
            };
            var versionLabel = new Label("1.4.0 • October 2024")
            {
                style =
                {
                    fontSize = 12,
                    height = 18,
                    paddingBottom = 2,
                    paddingLeft = 2,
                    paddingRight = 2,
                    paddingTop = 1,
                    unityFontStyleAndWeight = FontStyle.Bold
                }
            };
            versionContainer.Add(versionLabel);
            detailsHeader.Add(versionContainer);

            // Author.
            var authorLabel = new Label("By Alexander Ameye")
            {
                style =
                {
                    fontSize = 12,
                    marginBottom = 2,
                    marginLeft = 4,
                    marginRight = 4,
                    marginTop = 2,
                    paddingBottom = 2,
                    paddingLeft = 2,
                    paddingRight = 2,
                    paddingTop = 1
                }
            };
            detailsHeader.Add(authorLabel);

            // Links.
            var linksContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    paddingBottom = 5,
                    alignItems = Align.FlexStart
                }
            };
            var linksContainerHorizontal = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                }
            };
            var separator1 = new Label("|")
            {
                style =
                {
                    height = 20,
                    marginLeft = 4,
                    fontSize = 15,
                    color = new Color(0.1686275f, 0.1607843f, 0.1686275f),
                }
            };
            var separator2 = new Label("|")
            {
                style =
                {
                    height = 20,
                    marginLeft = 4,
                    fontSize = 15,
                    color = new Color(0.1686275f, 0.1607843f, 0.1686275f),
                }
            };
            var documentationLink = new Label
            {
                text = "Documentation",
                style =
                {
                    color = new StyleColor(new Color(0.2980392f, 0.4941176f, 1.0f, 1.0f)),
                    marginLeft = 6,
                    paddingLeft = 0,
                    paddingRight = 0
                }
            };
            var supportLink = new Label
            {
                text = "Support",
                style =
                {
                    color = new StyleColor(new Color(0.2980392f, 0.4941176f, 1.0f, 1.0f)),
                    marginLeft = 6,
                    paddingLeft = 0,
                    paddingRight = 0
                }
            };
            var reviewLink = new Label
            {
                text = "Review",
                style =
                {
                    color = new StyleColor(new Color(0.2980392f, 0.4941176f, 1.0f, 1.0f)),
                    marginLeft = 6,
                    paddingLeft = 0,
                    paddingRight = 0
                }
            };

            documentationLink.AddManipulator(new Clickable(() => Application.OpenURL("https://caustics.ameye.dev")));
            supportLink.AddManipulator(new Clickable(() => Application.OpenURL("https://discord.gg/cFfQGzQdPn")));
            reviewLink.AddManipulator(new Clickable(() => Application.OpenURL("https://assetstore.unity.com/packages/vfx/shaders/water-caustics-for-urp-221106#reviews")));
            linksContainerHorizontal.Add(documentationLink);
            linksContainerHorizontal.Add(separator1);
            linksContainerHorizontal.Add(supportLink);
            linksContainerHorizontal.Add(separator2);
            linksContainerHorizontal.Add(reviewLink);
            linksContainer.Add(linksContainerHorizontal);
            detailsHeader.Add(linksContainer);

            // List view.
            var compatibilityContainer = new VisualElement
            {
                style =
                {

                    backgroundColor = new Color(0.1686275f, 0.1607843f, 0.1686275f),
                    flexDirection = FlexDirection.Column,
                    flexGrow = 1,
                }
            };
            var child = new VisualElement();

            var descriptionLabel = new Label
            {
                text = "Select a check to see its description.",
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Italic,
                    marginTop = 10,
                    textOverflow = TextOverflow.Ellipsis,
                    whiteSpace = WhiteSpace.Normal,
                    paddingLeft = 5
                }
            };

            var listView = new ListView(checks, 20, MakeItem, BindItem)
            {
                style =
                {
                    flexGrow = 1.0f
                },  
                showAlternatingRowBackgrounds = AlternatingRowBackground.All,
                selectionType = SelectionType.Single
            };
            
            listView.selectionChanged += objects =>
            {
                if (objects.ToArray()[0] is Check selectedCheck)
                {
                    descriptionLabel.text = selectedCheck.Result.Description;
                }
            };

            var buttons = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    justifyContent = Justify.Center,
                    marginTop = 10
                }
            };
            var detectButton = new Button(() =>
            {
                foreach (var checkItem in checks)
                {
                    RunCheck(checkItem, listView);
                }
            })
            {
                text = "Check Compatibility",
                style =
                {

                    marginTop = 20,
                    marginLeft = 0,
                    marginRight = 0,
                    flexGrow = 1
                }

            };
            buttons.Add(detectButton);
            
            var copyInfoButton = new Button(() =>
            {
                CopyCheckInfoToClipboard();
            })
            {
                text = "Copy Result",
                style =
                {
                    marginLeft = 0,
                    marginRight = 0,
                    flexGrow = 1
                }
            };
            buttons.Add(copyInfoButton);
            
            container.Add(detailsHeader);

            child.Add(listView);
            child.Add(descriptionLabel);
            child.Add(buttons);
            compatibilityContainer.Add(child);
            container.Add(compatibilityContainer);
            
            rootVisualElement.Add(container);
            return;

            void BindItem(VisualElement e, int i)
            {
                var labels = e.Query<Label>().ToList();
                var image = e.Q<Image>();

                labels[0].text = checks[i].Name;
                labels[1].text = checks[i].Status switch
                {
                    CheckStatus.Untested => "Not tested",
                    CheckStatus.InProgress => "Testing...",
                    CheckStatus.Completed when checks[i].Result != null => checks[i].Result.Message,
                    _ => labels[1].text
                };

                image.image = checks[i].Status == CheckStatus.Completed && checks[i].Result != null
                    ? LoadIconForResult(checks[i].Result.Result).image
                    : LoadIconForResult(ResultEnum.Untested).image;
            }

            VisualElement MakeItem()
            {
                var row = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        paddingLeft = 4,
                        paddingRight = 4
                    }
                };

                var checkLabel = new Label
                {
                    style =
                    {
                        unityTextAlign = TextAnchor.MiddleLeft,
                        flexGrow = 1,
                        minWidth = 140
                    }
                };


                var statusLabel = new Label
                {
                    style =
                    {
                        unityTextAlign = TextAnchor.MiddleLeft,
                        flexGrow = 1,
                        minWidth = 110
                    }
                };


                var resultIcon = new Image()
                {
                    style =
                    {
                        unityTextAlign = TextAnchor.MiddleRight,
                        alignSelf = Align.Center,
                        justifyContent = Justify.FlexEnd,
                        minWidth = 20
                    }
                };

                row.Add(checkLabel);
                row.Add(statusLabel);
                row.Add(resultIcon);
                row.AddToClassList("list-view-row");

                return row;
            }
        }

        private void RunCheck(Check check, ListView listView)
        {
            check.Status = CheckStatus.InProgress;
            listView.RefreshItems();
            
            EditorApplication.delayCall += () =>
            {
                var result = check.RunCheck();
                check.Status = CheckStatus.Completed;
                check.Result = result;
                listView.RefreshItems();
            };
        }
        
        private void CopyCheckInfoToClipboard()
        {
            var stringBuilder = new StringBuilder();

            foreach (var check in checks)
            {
                if (check.Status == CheckStatus.Completed && check.Result != null)
                {
                    switch (check.Result.Result)
                    {
                        case ResultEnum.Pass:
                            stringBuilder.AppendLine($"PASS\t{check.Name}: {check.Result.Message}");
                            break;
                        case ResultEnum.Warning:
                            stringBuilder.AppendLine($"WARN\t{check.Name}: {check.Result.Message}");
                            break;
                        case ResultEnum.Fail:
                            stringBuilder.AppendLine($"FAIL\t{check.Name}: {check.Result.Message}");
                            break;
                    }
                    stringBuilder.AppendLine($"\t\t{check.Result.Description}");
                }
                else
                {
                    stringBuilder.AppendLine($"UNKNOWN\t{check.Name}");
                    stringBuilder.AppendLine("\t\tPlease run the compatibility check first.");
                }
                stringBuilder.AppendLine();
            }
            
            EditorGUIUtility.systemCopyBuffer = stringBuilder.ToString();
            Debug.Log("Compatibility info copied to clipboard!");
        }

        private CheckResult CheckUnityVersion()
        {
            var unityVersion = Application.unityVersion;
            if (unityVersion.StartsWith("6000"))
            {
                return new CheckResult(ResultEnum.Pass, unityVersion, "Water Caustics for URP is compatible with Unity 6.");
            }
            if (unityVersion.StartsWith("2022.3"))
            {
                return new CheckResult(ResultEnum.Warning, unityVersion, "Unity no longer develops or improves the rendering path that does not use Render Graph API. Upgrade to Unity 6 to make use of the Render Graph API.");
            }
            return new CheckResult(ResultEnum.Pass, unityVersion, $"Water Caustics for URP is not compatible with Unity {unityVersion}.");
        }
        
        private CheckResult CheckURPVersion()
        {
            _urpSearchRequest = Client.Search("com.unity.render-pipelines.universal", Application.internetReachability == NetworkReachability.NotReachable);
            while (!_urpSearchRequest.IsCompleted)
            {
                System.Threading.Thread.Sleep(100);
            }

            if (_urpSearchRequest.Status != StatusCode.Success)
            {
                return new CheckResult(ResultEnum.Fail, $"Error: {_urpSearchRequest.Error.message}", "An error occurred.");
            }
            
            var package = _urpSearchRequest.Result[0];
            var version = package.version;
            return new CheckResult(version.StartsWith("14") || version.StartsWith("17") ? ResultEnum.Pass : ResultEnum.Fail, version, "Water Caustics for URP is compatible with URP 14 and URP 17.");
        }

        private CheckResult CheckTargetPlatform()
        {
            var target = EditorUserBuildSettings.activeBuildTarget;
            return target switch
            {
                BuildTarget.StandaloneWindows or BuildTarget.StandaloneWindows64 => new CheckResult(ResultEnum.Pass, "Windows", "Water Caustics for URP is compatible with Windows."),
                BuildTarget.StandaloneOSX => new CheckResult(ResultEnum.Pass, "macOS", "Water Caustics for URP is compatible with macOS."),
                _ => new CheckResult(ResultEnum.Warning, BuildTarget.StandaloneWindows.ToString(),
                    $"Compatibility with {BuildTarget.StandaloneWindows.ToString()} has not yet been tested.")
            };
        }
        
        private CheckResult CheckActiveRenderer()
        {
            if (GraphicsSettings.defaultRenderPipeline == null) return new CheckResult(ResultEnum.Fail, "Not found", NoActiveRendererFoundDescription);
            var type = GraphicsSettings.defaultRenderPipeline.GetType().ToString();
            if (type.Contains("HDRenderPipelineAsset"))
                return new CheckResult(ResultEnum.Fail, "High Definition", "Water Caustics for URP is not compatible with the High Definition Render Pipeline.");
            if (type.Contains("UniversalRenderPipelineAsset")) return new CheckResult(ResultEnum.Pass, "Universal", "Water Caustics for URP is compatible with the Universal Render Pipeline.");
            if (type.Contains("LightweightRenderPipelineAsset"))
                return new CheckResult(ResultEnum.Fail, "Light Weight", "Water Caustics for URP is not compatible with the Light Weight Render Pipeline");
            return new CheckResult(ResultEnum.Fail, "Custom", "Water Caustics for URP is not compatible with any Custom Render Pipeline.");
        }
        
        private CheckResult CheckRenderGraph()
        {
#if UNITY_6000_0_OR_NEWER
            var renderGraphSettings = GraphicsSettings.GetRenderPipelineSettings<RenderGraphSettings>();
            var usingRenderGraph = !renderGraphSettings.enableRenderCompatibilityMode;
            return usingRenderGraph ? new CheckResult(ResultEnum.Pass,  "Enabled", "Render Graph is enabled.") : new CheckResult(ResultEnum.Warning, "Compatibility Mode", "Render Graph is available but not in use because Compatibility Mode is enabled. Unity no longer develops or improves the rendering path that does not use Render Graph API. See Edit > Project Settings > Graphics > Render Graph to disable Compatibility Mode.");
#else
            return new CheckResult(ResultEnum.Warning, "Not Available", "Unity no longer develops or improves the rendering path that does not use Render Graph API. Upgrade to Unity 6 to make use of the Render Graph API.");
#endif
        }
        
        private CheckResult CheckRenderingPath()
        {
            if (GraphicsSettings.defaultRenderPipeline == null) return new CheckResult(ResultEnum.Fail, "Not found", NoActiveRendererFoundDescription);
            var type = GraphicsSettings.defaultRenderPipeline.GetType().ToString();
            if (!type.Contains("UniversalRenderPipelineAsset")) return new CheckResult(ResultEnum.Fail, "URP renderer not found", "URP renderer not found.");
            var pipeline = (UniversalRenderPipelineAsset) GraphicsSettings.currentRenderPipeline;
            var propertyInfo = pipeline.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
            var scriptableRendererData = ((ScriptableRendererData[]) propertyInfo?.GetValue(pipeline))?[0];
            var renderingMode = ((UniversalRendererData) scriptableRendererData)!.renderingMode;
            return renderingMode switch
            {
                RenderingMode.Forward => new CheckResult(ResultEnum.Pass, "Forward", "Water Caustics for URP is compatible with the Forward rendering path."),
                RenderingMode.ForwardPlus => new CheckResult(ResultEnum.Pass, "Forward+", "Water Caustics for URP is compatible with the Forward+ rendering path."),
                RenderingMode.Deferred => new CheckResult(ResultEnum.Fail, "Deferred",
                    "Water Caustics for URP is not compatible with the Deferred rendering path. You can change the active rendering path on your active Render Pipeline asset."),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private CheckResult CheckGraphicsAPI()
        {
            return SystemInfo.graphicsDeviceType switch
            {
                GraphicsDeviceType.Metal => new CheckResult(ResultEnum.Pass, "Metal", "Water Caustics for URP is compatible with Metal."),
                GraphicsDeviceType.Vulkan => new CheckResult(ResultEnum.Pass, "Vulkan", "Water Caustics for URP is compatible with Vulkan."),
                GraphicsDeviceType.Direct3D11 => new CheckResult(ResultEnum.Pass, "Direct3D11", "Water Caustics for URP is compatible with Direct3D11."),
                GraphicsDeviceType.OpenGLCore => new CheckResult(ResultEnum.Fail, "OpenGLCore", "Water Caustics for URP is not compatible with OpenGLCore."),
                _ => new CheckResult(ResultEnum.Warning, SystemInfo.graphicsDeviceType.ToString(),
                    $"Compatibility with {SystemInfo.graphicsDeviceType.ToString()} has not been tested.")
            };
        }
        
        private CheckResult CheckDepthTexture()
        {
            if (GraphicsSettings.defaultRenderPipeline == null) return new CheckResult(ResultEnum.Fail, "Not found", NoActiveRendererFoundDescription);
            var type = GraphicsSettings.defaultRenderPipeline.GetType().ToString();
            if (!type.Contains("UniversalRenderPipelineAsset")) return new CheckResult(ResultEnum.Fail, "URP renderer not found", "URP renderer not found.");
            return ((UniversalRenderPipelineAsset) GraphicsSettings.currentRenderPipeline).supportsCameraDepthTexture ? new CheckResult(ResultEnum.Pass, "Enabled", "The depth texture is enabled.") : new CheckResult(ResultEnum.Fail, "Disabled", "The depth texture is disabled. See your Universal Render Pipeline asset to enable it.");
        }
        
        private CheckResult CheckOpaqueTexture()
        {
            if (GraphicsSettings.defaultRenderPipeline == null) return new CheckResult(ResultEnum.Fail, "Not found", NoActiveRendererFoundDescription);
            var type = GraphicsSettings.defaultRenderPipeline.GetType().ToString();
            if (!type.Contains("UniversalRenderPipelineAsset")) return new CheckResult(ResultEnum.Fail, "URP renderer not found", "URP renderer not found.");
            return ((UniversalRenderPipelineAsset) GraphicsSettings.currentRenderPipeline).supportsCameraOpaqueTexture ? new CheckResult(ResultEnum.Pass, "Enabled", "The opaque texture is enabled.") : new CheckResult(ResultEnum.Fail, "Disabled", "The opaque texture is disabled. See your Universal Render Pipeline asset to enable it.");
        }
        
        private CheckResult CheckCausticsPass()
        {
            if (GraphicsSettings.defaultRenderPipeline == null) return new CheckResult(ResultEnum.Fail, "Not found", NoActiveRendererFoundDescription);
            var type = GraphicsSettings.defaultRenderPipeline.GetType().ToString();
            if (!type.Contains("UniversalRenderPipelineAsset")) return new CheckResult(ResultEnum.Fail, "URP renderer not found", "URP renderer not found.");
            var pipeline = (UniversalRenderPipelineAsset) GraphicsSettings.currentRenderPipeline;
            var propertyInfo = pipeline.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
            var scriptableRendererData = ((ScriptableRendererData[]) propertyInfo?.GetValue(pipeline))?[0];
            var causticsLightDirectionFeature = scriptableRendererData.rendererFeatures.OfType<Caustics>().FirstOrDefault();
            return causticsLightDirectionFeature != null
                ? causticsLightDirectionFeature.isActive ? new CheckResult(ResultEnum.Pass, "Enabled", "The caustics pass is present and enabled.") : new CheckResult(ResultEnum.Warning, "Disabled", "The caustics pass is present but disabled. See your Universal Renderer Data asset to enable it.")
                : new CheckResult(ResultEnum.Warning, "Missing", "The caustics pass is missing. See your Universal Renderer Data asset to add it.");
        }
        
        private class Check
        {
            public string Name { get; }
            public CheckStatus Status { get; set; }
            public CheckResult Result { get; set; }
            private Func<CheckResult> CheckCallback { get; }

            public Check(string name, Func<CheckResult> checkCallback)
            {
                Name = name;
                Status = CheckStatus.Untested;
                Result = null;
                CheckCallback = checkCallback;
            }

            public CheckResult RunCheck()
            {
                return CheckCallback.Invoke();
            }
        }

        private static GUIContent LoadIconForResult(ResultEnum result)
        {
            var iconName = result switch
            {
                ResultEnum.Untested => "TestIgnored",
                ResultEnum.Pass => "d_GreenCheckmark",
                ResultEnum.Fail => "d_console.erroricon.sml",
                ResultEnum.Warning => "d_console.warnicon.sml",
                _ => null
            };

            return !string.IsNullOrEmpty(iconName) ? EditorGUIUtility.IconContent(iconName) : null;
        }
    }
}