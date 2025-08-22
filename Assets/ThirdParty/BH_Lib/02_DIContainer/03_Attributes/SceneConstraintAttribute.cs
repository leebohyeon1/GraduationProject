using System;

namespace BH_Lib.DI
{
    /// <summary>
    /// 객체가 특정 씬에서만 생성되도록 제약하는 어트리뷰트
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SceneConstraintAttribute : Attribute
    {
        /// <summary>
        /// 객체가 생성될 수 있는 씬 이름들
        /// </summary>
        public string[] SceneNames { get; private set; }
        
        /// <summary>
        /// 객체가 생성될 수 있는 씬 빌드 인덱스들
        /// </summary>
        public int[] SceneIndices { get; private set; }

        /// <summary>
        /// 지정된 씬 이름에서만 객체를 생성합니다.
        /// </summary>
        /// <param name="sceneNames">객체가 생성될 씬 이름들</param>
        public SceneConstraintAttribute(params string[] sceneNames)
        {
            SceneNames = sceneNames;
            SceneIndices = new int[0];
        }

        /// <summary>
        /// 지정된 씬 인덱스에서만 객체를 생성합니다.
        /// </summary>
        /// <param name="sceneIndices">객체가 생성될 씬 빌드 인덱스들</param>
        public SceneConstraintAttribute(params int[] sceneIndices)
        {
            SceneNames = new string[0];
            SceneIndices = sceneIndices;
        }
    }
}
