using UnityEngine;

namespace Assets.PseudoCardboard
{
    public class MeshParameters
    {
        private static MeshParameters _instance = null;

        public const string SegmentWidthFieldName = "Mesh.SegmentWidth";
        public const string SegmentHeightFieldName = "Mesh.SegmentHeight";

        public int SegmentWidth { get; set; }
        public int SegmentHeight { get; set; }

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
        { }

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