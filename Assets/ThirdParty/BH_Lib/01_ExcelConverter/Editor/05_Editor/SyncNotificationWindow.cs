using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace BH_Lib.ExcelConverter.Editor
{
    /// <summary>
    /// 사용자가 엑셀 파일을 닫을 때까지 대기하는 알림 창
    /// </summary>
    public class SyncNotificationWindow : EditorWindow
    {
        private string _fileName;
        private string _filePath;
        private Action _onClosedCallback;
        private Action _onOpenCallback;
        private bool _isChecking = false;
        private float _lastCheckTime;
        private readonly float _checkInterval = 1.0f; // 1초마다 확인
        private bool _closedProgrammatically = false;
        /// <summary>
        /// 알림 창 생성
        /// </summary>
        /// <param name="fileName">엑셀 파일 이름</param>
        /// <param name="filePath">엑셀 파일 경로</param>
        /// <param name="onClosedCallback">파일이 닫힌 후 호출될 콜백</param>
        public SyncNotificationWindow(string fileName, string filePath, Action onClosedCallback, Action onOpenCallback)
        {
            _fileName = fileName;
            _filePath = filePath;
            _onClosedCallback = onClosedCallback;
            _onOpenCallback = onOpenCallback;
            _lastCheckTime = Time.realtimeSinceStartup;
        }

        /// <summary>
        /// 알림 창 초기화
        /// </summary>
        private void OnEnable()
        {
            titleContent = new GUIContent("엑셀 파일 동기화 대기 중");
            minSize = new Vector2(400, 150);
            maxSize = new Vector2(500, 200);
            position = new Rect(
                (Screen.currentResolution.width - minSize.x) / 2,
                (Screen.currentResolution.height - minSize.y) / 2,
                minSize.x,
                minSize.y
            );
        }

        /// <summary>
        /// GUI 그리기
        /// </summary>
        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            GUILayout.Label($"'{_fileName}' 파일이 현재 열려있습니다", EditorStyles.boldLabel);
            
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "1. 엑셀 프로그램에서 파일을 저장 후 닫아주세요.\n" +
                "2. 파일이 닫히면 자동으로 동기화가 진행됩니다.\n" +
                "3. 작업을 완료하기 위해 이 창을 유지해주세요.", 
                MessageType.Info
            );
            
            EditorGUILayout.Space(10);
            
            EditorGUI.BeginDisabledGroup(_isChecking);
            if (GUILayout.Button("파일 확인하기", GUILayout.Height(30)))
            {
                CheckFileStatus();
            }
            EditorGUI.EndDisabledGroup();

            // 주기적으로 자동 체크
            if (Event.current.type == EventType.Repaint)
            {
                float currentTime = Time.realtimeSinceStartup;
                if (currentTime - _lastCheckTime > _checkInterval)
                {
                    _lastCheckTime = currentTime;
                    CheckFileStatus();
                }
            }
        }

        /// <summary>
        /// 창이 닫히기 전 확인
        /// </summary>
        private void OnLostFocus()
        {
            // 창이 포커스를 잃었을 때 상태 기록
            // 이는 X 버튼을 누를 때도 발생함
        }

        /// <summary>
        /// OnDestroy 이벤트 처리 - 창이 닫힐 때 역방향 동기화 수행
        /// </summary>
        private void OnDestroy()
        {
            // 프로그래밍 방식으로 닫히지 않은 경우에만 역방향 동기화 실행
            if (!_closedProgrammatically)
            {
                // 창이 X 버튼이나 다른 방법으로 닫힐 때 역방향 동기화
                EditorApplication.delayCall += () => {
                        _onOpenCallback?.Invoke();   
                };
            }
        }
        /// <summary>
        /// 프로그래밍 방식으로 창 닫기
        /// </summary>
        public new void Close()
        {
            _closedProgrammatically = true;
            base.Close();
        }

        /// <summary>
        /// 파일 상태 확인
        /// </summary>
        private void CheckFileStatus()
        {
            _isChecking = true;
            
            // 파일이 잠겨있는지 확인
            if (!IsFileLocked())
            {
                _isChecking = false;
                Close();
                _onClosedCallback?.Invoke();
            }
            else
            {
                _isChecking = false;
                Repaint();
            }
        }

        /// <summary>
        /// 파일 잠금 여부 확인
        /// </summary>
        /// <returns>파일이 잠겼는지 여부</returns>
        private bool IsFileLocked()
        {
            if (!File.Exists(_filePath))
                return false;
                
            try
            {
                using (FileStream stream = File.Open(_filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    stream.Close();
                    return false;
                }
            }
            catch (IOException)
            {
                return true;
            }
            catch (Exception)
            {
                return true;
            }
        }
    }
}