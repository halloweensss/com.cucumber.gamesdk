﻿using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using GameSDK.Authentication;
using GameSDK.Core;
using GameSDK.Core.Properties;
using GameSDK.GameFeedback;
using UnityEngine;

namespace GameSDK.Plugins.YaGames.Feedback
{
    public class YaFeedback : IFeedbackApp
    {
        private static readonly YaFeedback _instance = new YaFeedback();

        private YaFailReviewReason _failReviewReason;
        private bool _result;
        private ReviewStatus _status;
        public PlatformServiceType PlatformService => PlatformServiceType.YaGames;
        public InitializationStatus InitializationStatus => InitializationStatus.Initialized;
        public async Task<(bool, FailReviewReason)> CanReview()
        {
#if !UNITY_EDITOR
            _status = ReviewStatus.Waiting;
            YaGamesCanReview(OnSuccess, OnError);

            while (_status == ReviewStatus.Waiting)
                await Task.Yield();

            return (_result, GetReason(_failReviewReason));
#else
            _status = ReviewStatus.Waiting;
            OnSuccess();
            await Task.CompletedTask;
            return (_result, GetReason(_failReviewReason));
#endif

            [MonoPInvokeCallback(typeof(Action))]
            static void OnSuccess()
            {
                _instance._result = true;
                _instance._failReviewReason = YaFailReviewReason.Unknown;
                _instance._status = ReviewStatus.Success;

                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Feedback]: YaFeedback review possible!");
                }
            }

            [MonoPInvokeCallback(typeof(Action<int>))]
            static void OnError(int reason)
            {
                _instance._result = false;
                _instance._failReviewReason = (YaFailReviewReason)reason;
                _instance._status = ReviewStatus.Error;

                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Feedback]: YaFeedback review impossible!");
                }
            }
        }

        public async Task<(bool, FailReviewReason)> RequestReview()
        {
            var canReview = await CanReview();

            if (canReview.Item1 == false)
            {
                if (canReview.Item2 == FailReviewReason.NoAuth)
                {
                    await Auth.SignIn();

                    if (Auth.IsAuthorized)
                    {
                        canReview = await CanReview();

                        if (canReview.Item1 == false)
                        {
                            return canReview;
                        }
                    }
                    else
                    {
                        if (GameApp.IsDebugMode)
                        {
                            Debug.Log($"[GameSDK.Feedback]: Before leaving a review, log in YaFeedback!");
                        }

                        return canReview;
                    }
                }
                else
                {
                    return canReview;
                }
            }
            
#if !UNITY_EDITOR
            _status = ReviewStatus.Waiting;
            YaGamesRequestReview(OnSuccess, OnError);

            while (_status == ReviewStatus.Waiting)
                await Task.Yield();

            return (_result, GetReason(_failReviewReason));
#else
            _status = ReviewStatus.Waiting;
            OnSuccess();
            await Task.CompletedTask;
            return (_result, GetReason(_failReviewReason));
#endif

            [MonoPInvokeCallback(typeof(Action))]
            static void OnSuccess()
            {
                _instance._result = true;
                _instance._failReviewReason = YaFailReviewReason.Unknown;
                _instance._status = ReviewStatus.Success;

                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Feedback]: YaFeedback review has been delivered!");
                }
            }

            [MonoPInvokeCallback(typeof(Action<int>))]
            static void OnError(int reason)
            {
                _instance._result = false;
                _instance._failReviewReason = (YaFailReviewReason)reason;
                _instance._status = ReviewStatus.Error;

                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Feedback]: YaFeedback review was not submitted!");
                }
            }
        }

        private FailReviewReason GetReason(YaFailReviewReason reason) =>
            reason switch
            {
                YaFailReviewReason.Unknown => FailReviewReason.Unknown,
                YaFailReviewReason.NoAuth => FailReviewReason.NoAuth,
                YaFailReviewReason.GameRated => FailReviewReason.GameRated,
                YaFailReviewReason.ReviewAlreadyRequested => FailReviewReason.Unknown,
                YaFailReviewReason.ReviewWasRequested => FailReviewReason.Unknown,
                YaFailReviewReason.Canceled => FailReviewReason.Canceled,
                _ => FailReviewReason.Unknown
            };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterInternal()
        {
            GameFeedback.Feedback.Instance.Register(_instance);
        }
        
        [DllImport("__Internal")]
        private static extern void YaGamesCanReview(Action onSuccess, Action<int> onError);
        
        [DllImport("__Internal")]
        private static extern void YaGamesRequestReview(Action onSuccess, Action<int> onError);
    }
}