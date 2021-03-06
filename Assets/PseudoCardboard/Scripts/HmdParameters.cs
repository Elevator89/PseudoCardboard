﻿/* 
 * Copyright 2018 Andrey Lemin
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using UnityEngine;
using UnityEngine.Events;

namespace Assets.PseudoCardboard
{
    public class HmdParameters
    {
        [System.Serializable]
        public class HmdParametersChangedEvent : UnityEvent<HmdParameters>
        {
        }

        private static HmdParameters _instance = null;

        private const string DistortionK1FieldName = "Hmd.DistortionK1";
        private const string DistortionK2FieldName = "Hmd.DistortionK2";
        private const string InterlensDistanceFieldName = "Hmd.InterlensDistance";
        private const string ScreenToLensDistFieldName = "Hmd.ScreenToLensDist";
        private const string EyeOffsetYFieldName = "Hmd.EyeOffsetY";

        private const string MaxFovAnglesLeftFieldName = "Hmd.MaxFovAngles.FovLeft";
        private const string MaxFovAnglesRightFieldName = "Hmd.MaxFovAngles.FovRight";
        private const string MaxFovAnglesTopFieldName = "Hmd.MaxFovAngles.FovTop";
        private const string MaxFovAnglesBottomFieldName = "Hmd.MaxFovAngles.FovBottom";

        private static readonly HmdParameters DefaultValues = new HmdParameters()
        {
            DistortionK1 = 0.51f,
            DistortionK2 = 0.16f,
            ScreenToLensDist = 0.045f,
            InterlensDistance = 0.065f,
            EyeOffsetY = 0.035f,
            MaxFovAngles = new Fov
            {
                Left = PlayerPrefsExt.SafeGetFloat("FovLeft", 50),
                Right = PlayerPrefsExt.SafeGetFloat("FovRight", 50),
                Top = PlayerPrefsExt.SafeGetFloat("FovTop", 50),
                Bottom = PlayerPrefsExt.SafeGetFloat("FovBottom", 50)
            }
        };

        public static HmdParameters Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new HmdParameters();
                    _instance.LoadFromPrefs();
                }
                return _instance;
            }
        }

        private float _distortionK1;
        private float _distortionK2;
        private float _interlensDistance;
        private float _screenToLensDist;
        private float _eyeOffsetY;
        private Fov _maxFovAngles;

        public HmdParametersChangedEvent ParamsChanged;

        public float DistortionK1
        {
            get { return _distortionK1; }
            set
            {
                _distortionK1 = value;
                ParamsChanged.Invoke(this);
            }
        }

        public float DistortionK2
        {
            get { return _distortionK2; }
            set
            {
                _distortionK2 = value;
                ParamsChanged.Invoke(this);
            }
        }

        public float InterlensDistance
        {
            get { return _interlensDistance; }
            set
            {
                _interlensDistance = value;
                ParamsChanged.Invoke(this);
            }
        }

        public float ScreenToLensDist
        {
            get { return _screenToLensDist; }
            set
            {
                _screenToLensDist = value;
                ParamsChanged.Invoke(this);
            }
        }

        public float EyeOffsetY
        {
            get { return _eyeOffsetY; }
            set
            {
                _eyeOffsetY = value;
                ParamsChanged.Invoke(this);
            }
        }

        public Fov MaxFovAngles
        {
            get { return _maxFovAngles; }
            set
            {
                _maxFovAngles = value;
                ParamsChanged.Invoke(this);
            }
        }

        private HmdParameters()
        {
            if (ParamsChanged == null)
                ParamsChanged = new HmdParametersChangedEvent();
        }

        public void SaveToPrefs()
        {
            PlayerPrefs.SetFloat(ScreenToLensDistFieldName, ScreenToLensDist);
            PlayerPrefs.SetFloat(DistortionK1FieldName, DistortionK1);
            PlayerPrefs.SetFloat(DistortionK2FieldName, DistortionK2);
            PlayerPrefs.SetFloat(InterlensDistanceFieldName, InterlensDistance);
            PlayerPrefs.SetFloat(EyeOffsetYFieldName, EyeOffsetY);

            PlayerPrefs.SetFloat(MaxFovAnglesLeftFieldName, MaxFovAngles.Left);
            PlayerPrefs.SetFloat(MaxFovAnglesRightFieldName, MaxFovAngles.Right);
            PlayerPrefs.SetFloat(MaxFovAnglesTopFieldName, MaxFovAngles.Top);
            PlayerPrefs.SetFloat(MaxFovAnglesBottomFieldName, MaxFovAngles.Bottom);
        }

        public void LoadFromPrefs()
        {
            ScreenToLensDist = PlayerPrefsExt.SafeGetFloat(ScreenToLensDistFieldName, DefaultValues.ScreenToLensDist);
            DistortionK1 = PlayerPrefsExt.SafeGetFloat(DistortionK1FieldName, DefaultValues.DistortionK1);
            DistortionK2 = PlayerPrefsExt.SafeGetFloat(DistortionK2FieldName, DefaultValues.DistortionK2);
            InterlensDistance = PlayerPrefsExt.SafeGetFloat(InterlensDistanceFieldName, DefaultValues.InterlensDistance);
            EyeOffsetY = PlayerPrefsExt.SafeGetFloat(EyeOffsetYFieldName, DefaultValues.EyeOffsetY);
            MaxFovAngles = new Fov
            {
                Left = PlayerPrefsExt.SafeGetFloat(MaxFovAnglesLeftFieldName, DefaultValues.MaxFovAngles.Left),
                Right = PlayerPrefsExt.SafeGetFloat(MaxFovAnglesRightFieldName, DefaultValues.MaxFovAngles.Right),
                Top = PlayerPrefsExt.SafeGetFloat(MaxFovAnglesTopFieldName, DefaultValues.MaxFovAngles.Top),
                Bottom = PlayerPrefsExt.SafeGetFloat(MaxFovAnglesBottomFieldName, DefaultValues.MaxFovAngles.Bottom)
            };
        }

        public void LoadDefaults()
        {
            ScreenToLensDist = DefaultValues.ScreenToLensDist;
            DistortionK1 = DefaultValues.DistortionK1;
            DistortionK2 = DefaultValues.DistortionK2;
            InterlensDistance = DefaultValues.InterlensDistance;
            EyeOffsetY = DefaultValues.EyeOffsetY;
            MaxFovAngles = DefaultValues.MaxFovAngles;
        }
    }
}
