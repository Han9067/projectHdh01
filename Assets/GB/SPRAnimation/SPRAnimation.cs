using System;
using QuickEye.Utility;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace GB
{

    public class SPRAnimation : MonoBehaviour
    {

        [SerializeField] SPRAnimationClip _SPRAnimation;
        [SerializeField] UnityDictionary<string, SPRAnimationClip> _SPRAnimations;

        Image _img;

        SpriteRenderer _sprRender;

        public bool PlayAutomatically = true;
        [SerializeField] string _skinName;


        public UnityEvent PlayEvent;
        public UnityEvent EndEvent;
        public UnityEvent<TriggerData> TriggerEvent;


        void Awake()
        {
            _sprRender = GetComponent<SpriteRenderer>();
            _img = GetComponent<Image>();
        }

        void OnEnable()
        {
            if (PlayAutomatically) Play();

        }

        void OnTriggerCallBack(State state, int idx, TriggerData triggerData)
        {
            switch (state)
            {
                case State.Play:
                    PlayEvent?.Invoke();
                    break;

                case State.End:
                    EndEvent?.Invoke();
                    break;

                case State.Trigger:
                    TriggerEvent?.Invoke(triggerData);
                    break;
            }
        }

        public void SetSkin(string skinName)
        {
            _skinName = skinName;
        }



        public void Play(float speed = 1)
        {
            if (_SPRAnimation == null) return;

            if (_SPRAnimation.GetSpriteCount(_skinName) <= 0) return;
            _curIDX = 0;
            if (_sprRender != null)
                _sprRender.sprite = _SPRAnimation.GetSprite(_curIDX);
            if (_img != null)
                _img.sprite = _SPRAnimation.GetSprite(_curIDX);
            _isPlaying = true;
            if (speed < 0) _gameSpeed = 1;

            _IsLoop = _SPRAnimation.IsLoop;
            _gameSpeed = _SPRAnimation.Speed * speed;
            _fixTimer = TIMER / (_SPRAnimation.Speed * speed);
            OnTriggerCallBack(State.Play, _curIDX, null);
        }

        public void Play(string animationName, float speed = 1)
        {

            if (_SPRAnimations.ContainsKey(animationName))
                _SPRAnimation = _SPRAnimations[animationName];
            else
                return;

            if (_SPRAnimation == null) return;

            if (_SPRAnimation.GetSpriteCount(_skinName) <= 0) return;
            _curIDX = 0;
            if (_sprRender != null)
                _sprRender.sprite = _SPRAnimation.GetSprite(_curIDX);
            if (_img != null)
                _img.sprite = _SPRAnimation.GetSprite(_curIDX);
            _isPlaying = true;
            if (speed < 0) _gameSpeed = 1;
            _IsLoop = _SPRAnimation.IsLoop;
            _gameSpeed = _SPRAnimation.Speed * speed;
            _fixTimer = TIMER / (_SPRAnimation.Speed * speed);
            OnTriggerCallBack(State.Play, _curIDX, null);
        }

        public void Play(SPRAnimationClip animation, float speed = 1)
        {
            _SPRAnimation = animation;
            if (_SPRAnimation == null) return;
            if (_SPRAnimation.GetSpriteCount(_skinName) <= 0) return;
            _curIDX = 0;
            if (_sprRender != null)
                _sprRender.sprite = _SPRAnimation.GetSprite(_curIDX);
            if (_img != null)
                _img.sprite = _SPRAnimation.GetSprite(_curIDX);
            _isPlaying = true;
            if (speed < 0) _gameSpeed = 1;
            _IsLoop = _SPRAnimation.IsLoop;
            _gameSpeed = _SPRAnimation.Speed * speed;
            _fixTimer = TIMER / (_SPRAnimation.Speed * speed);
            OnTriggerCallBack(State.Play, _curIDX, null);
        }

        public void Stop()
        {

            _isPlaying = false;
            OnTriggerCallBack(State.End, _curIDX, null);
        }

        public void Resume()
        {

            _isPlaying = true;
        }


        #region  Update

        float _gameSpeed;


        public enum State { Play = 0, Next, End, Trigger }

        public bool IsPlaying { get { return _isPlaying; } }
        bool _isPlaying;

        string _curID;

        public string AnimationName { get { return _curID; } }
        int _curIDX;
        public int AnimationIndex { get { return _curIDX; } }
        bool _IsLoop;
        public bool IsLoop { get { return _IsLoop; } }
        float _fixTimer;

        
        const float TIMER = 0.08f;





        float _time;
        void SPRUpdate(float dt)
        {
            if (_SPRAnimation == null) return;
            if (_isPlaying == false) return;

            _time += dt * _gameSpeed;

            if (_time > _fixTimer)
            {
                _time = 0;
                _curIDX++;

                if (_curIDX >= _SPRAnimation.GetSpriteCount(_skinName))
                {
                    if (_IsLoop)
                    {
                        _curIDX = 0;
                        if (_sprRender != null)
                            _sprRender.sprite = _SPRAnimation.GetSprite(_curIDX,_skinName);
                        if (_img != null)
                            _img.sprite = _SPRAnimation.GetSprite(_curIDX,_skinName);
                    }
                    else
                    {
                        Stop();
                        return;
                    }
                }

                if (_sprRender != null)
                    _sprRender.sprite = _SPRAnimation.GetSprite(_curIDX,_skinName);
                if (_img != null)
                    _img.sprite = _SPRAnimation.GetSprite(_curIDX,_skinName);

                if (_SPRAnimation.Triggers.ContainsKey(_curIDX))
                {
                    if (_SPRAnimation.Triggers != null)
                    {
                        for (int i = 0; i < _SPRAnimation.Triggers[_curIDX].Count; ++i)
                            OnTriggerCallBack(State.Trigger, _curIDX, _SPRAnimation.Triggers[_curIDX][i]);
                    }
                }
            }
        }


        const string GAME = "GAME";


        void Update()
        {
            if (_isPlaying == false) return;
            if (_SPRAnimation != null) SPRUpdate(GBTime.GetDeltaTime(GAME));
        }

        #endregion


    }
}
