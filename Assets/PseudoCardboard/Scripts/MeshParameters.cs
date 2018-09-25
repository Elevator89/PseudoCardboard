using System;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.PseudoCardboard
{
    public class MeshParameters
    {
        [System.Serializable]
        public class MeshParametersChangedEvent : UnityEvent<int, int>
        {
        }

        private static MeshParameters _instance = null;

        public const string SegmentWidthFieldName = "Mesh.SegmentWidth";
        public const string SegmentHeightFieldName = "Mesh.SegmentHeight";

        private int _segmentWidth;
        public int SegmentWidth
        {
            get { return _segmentWidth; }
            set
            {
                _segmentWidth = value;
                ParamsChanged.Invoke(_segmentWidth, _segmentHeight);
            }
        }

        private int _segmentHeight;
        public int SegmentHeight
        {
            get { return _segmentHeight; }
            set
            {
                _segmentHeight = value;
                ParamsChanged.Invoke(_segmentWidth, _segmentHeight);
            }
        }

        public MeshParametersChangedEvent ParamsChanged;

        private static readonly MeshParameters DefaultValues = new MeshParameters()
        {
            SegmentWidth = 8,
            SegmentHeight = 8,
        };

        public static MeshParameters Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MeshParameters();
                    _instance.LoadFromPrefs();
                }
                return _instance;
            }
        }

        private MeshParameters()
        {
            if (ParamsChanged == null)  
                ParamsChanged = new MeshParametersChangedEvent();
        }

        public void SaveToPrefs()
        {
            PlayerPrefs.SetInt(SegmentWidthFieldName, SegmentWidth);
            PlayerPrefs.SetInt(SegmentHeightFieldName, SegmentHeight);
        }

        public void LoadFromPrefs()
        {
            SegmentWidth = PlayerPrefsExt.SafeGetInt(SegmentWidthFieldName, DefaultValues.SegmentWidth);
            SegmentHeight = PlayerPrefsExt.SafeGetInt(SegmentHeightFieldName, DefaultValues.SegmentHeight);
        }

        public void LoadDefaults()
        {
            SegmentWidth = DefaultValues.SegmentWidth;
            SegmentHeight = DefaultValues.SegmentHeight;
        }
    }
}