using System;
using UnityEngine;
using UnityEngine.UI;
using BH_Lib.DI;

namespace ProjectStudio.DI.Examples.MVVM
{
    #region Models
    
    /// <summary>
    /// 사용자 데이터를 나타내는 모델
    /// </summary>
    public class UserModel
    {
        public string Username { get; set; }
        public int Score { get; set; }
        public DateTime LastLogin { get; set; }
        
        public UserModel(string username, int score)
        {
            Username = username;
            Score = score;
            LastLogin = DateTime.Now;
        }
    }
    
    /// <summary>
    /// 사용자 데이터 관리 서비스 인터페이스
    /// </summary>
    public interface IUserService
    {
        UserModel GetCurrentUser();
        void UpdateUserScore(int additionalScore);
        event Action<UserModel> OnUserUpdated;
    }
    
    /// <summary>
    /// 사용자 데이터 관리 서비스 구현
    /// </summary>
    [Register(typeof(IUserService))]
    public class UserService : IUserService
    {
        private UserModel _currentUser;
        
        public event Action<UserModel> OnUserUpdated;
        
        public UserService()
        {
            // 실제로는 로컬 스토리지나 서버에서 로드할 수 있음
            _currentUser = new UserModel("Player1", 100);
        }
        
        public UserModel GetCurrentUser()
        {
            return _currentUser;
        }
        
        public void UpdateUserScore(int additionalScore)
        {
            _currentUser.Score += additionalScore;
            _currentUser.LastLogin = DateTime.Now;
            OnUserUpdated?.Invoke(_currentUser);
        }
    }
    
    #endregion
    
    #region ViewModels
    
    /// <summary>
    /// 사용자 프로필 화면을 위한 ViewModel
    /// </summary>
    [Register]
    public class UserProfileViewModel
    {
        private readonly IUserService _userService;
        
        // 바인딩 가능한 프로퍼티와 이벤트
        public string Username => _userService.GetCurrentUser().Username;
        public int Score => _userService.GetCurrentUser().Score;
        public string LastLoginText => $"Last login: {_userService.GetCurrentUser().LastLogin:g}";
        
        public event Action OnUserDataChanged;

        public UserProfileViewModel(IUserService userService)
        {
            _userService = userService;
            _userService.OnUserUpdated += HandleUserUpdated;
        }
        
        ~UserProfileViewModel()
        {
            // 이벤트 구독 해제
            if (_userService != null)
            {
                _userService.OnUserUpdated -= HandleUserUpdated;
            }
        }
        
        private void HandleUserUpdated(UserModel user)
        {
            // 데이터가 변경되었음을 알림
            OnUserDataChanged?.Invoke();
        }
        
        public void AddPoints(int points)
        {
            _userService.UpdateUserScore(points);
        }
    }
    
    #endregion
    
    #region Views
    
    /// <summary>
    /// 사용자 프로필 UI를 표시하는 View 컴포넌트
    /// </summary>
    public class UserProfileView : DIMonoBehaviour
    {
        [SerializeField] private Text _usernameText;
        [SerializeField] private Text _scoreText;
        [SerializeField] private Text _lastLoginText;
        [SerializeField] private Button _addPointsButton;
        
        [Inject]
        private UserProfileViewModel _viewModel;
        
        protected override void Awake()
        {
            base.Awake();
            
            // 버튼 이벤트 등록
            if (_addPointsButton != null)
            {
                _addPointsButton.onClick.AddListener(OnAddPointsClicked);
            }
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            // ViewModel 이벤트 구독
            if (_viewModel != null)
            {
                _viewModel.OnUserDataChanged += UpdateUI;
            }
            
            // 초기 UI 업데이트
            UpdateUI();
        }
        
        private void OnDisable()
        {
            // ViewModel 이벤트 구독 해제
            if (_viewModel != null)
            {
                _viewModel.OnUserDataChanged -= UpdateUI;
            }
            
            // 버튼 이벤트 해제
            if (_addPointsButton != null)
            {
                _addPointsButton.onClick.RemoveListener(OnAddPointsClicked);
            }
        }
        
        private void UpdateUI()
        {
            if (_viewModel == null) return;
            
            if (_usernameText != null)
            {
                _usernameText.text = _viewModel.Username;
            }
            
            if (_scoreText != null)
            {
                _scoreText.text = $"Score: {_viewModel.Score}";
            }
            
            if (_lastLoginText != null)
            {
                _lastLoginText.text = _viewModel.LastLoginText;
            }
        }
        
        private void OnAddPointsClicked()
        {
            _viewModel?.AddPoints(10);
        }
    }
    
    #endregion
}
