using Assets.Scripts.Utils;
using UnityEditor;

namespace Assets.Editor {
    [CustomEditor(typeof(ExposableMonobehaviour), true)]
    // ReSharper disable once IdentifierTypo
    public class ExposableMonobehaviourEditor : UnityEditor.Editor {
        ExposableMonobehaviour _mInstance;
        PropertyField[] _mFields;

        // ReSharper disable once UnusedMember.Global
        public virtual void OnEnable() {
            _mInstance = target as ExposableMonobehaviour;
            _mFields = ExposeProperties.GetProperties(_mInstance);
        }

        public override void OnInspectorGUI() {
            if (_mInstance == null)
                return;
            DrawDefaultInspector();
            ExposeProperties.Expose(_mFields);
        }
    }
}