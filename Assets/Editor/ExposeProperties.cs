using System;
using System.Collections.Generic;
using System.Reflection;
using Assets.Scripts.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Editor {
    public static class ExposeProperties {
        public static void Expose(PropertyField[] properties) {
            var emptyOptions = new GUILayoutOption[0];
            EditorGUILayout.BeginVertical(emptyOptions);
            foreach (var field in properties) {
                EditorGUILayout.BeginHorizontal(emptyOptions);
                if (field.Type == SerializedPropertyType.Integer) {
                    var oldValue = (int) field.GetValue();
                    var newValue = EditorGUILayout.IntField(field.Name, oldValue, emptyOptions);
                    if (oldValue != newValue)
                        field.SetValue(newValue);
                }
                else if (field.Type == SerializedPropertyType.Float) {
                    var oldValue = (float) field.GetValue();
                    var newValue = EditorGUILayout.FloatField(field.Name, oldValue, emptyOptions);
                    if (oldValue != newValue)
                        field.SetValue(newValue);
                }
                else if (field.Type == SerializedPropertyType.Boolean) {
                    var oldValue = (bool) field.GetValue();
                    var newValue = EditorGUILayout.Toggle(field.Name, oldValue, emptyOptions);
                    if (oldValue != newValue)
                        field.SetValue(newValue);
                }
                else if (field.Type == SerializedPropertyType.String) {
                    var oldValue = (string) field.GetValue();
                    var newValue = EditorGUILayout.TextField(field.Name, oldValue, emptyOptions);
                    if (oldValue != newValue)
                        field.SetValue(newValue);
                }
                else if (field.Type == SerializedPropertyType.Vector2) {
                    var oldValue = (Vector2) field.GetValue();
                    var newValue = EditorGUILayout.Vector2Field(field.Name, oldValue, emptyOptions);
                    if (oldValue != newValue)
                        field.SetValue(newValue);
                }
                else if (field.Type == SerializedPropertyType.Vector3) {
                    var oldValue = (Vector3) field.GetValue();
                    var newValue = EditorGUILayout.Vector3Field(field.Name, oldValue, emptyOptions);
                    if (oldValue != newValue)
                        field.SetValue(newValue);
                }
                else if (field.Type == SerializedPropertyType.Enum) {
                    var oldValue = (Enum) field.GetValue();
                    var newValue = EditorGUILayout.EnumPopup(field.Name, oldValue, emptyOptions);
                    if (!Equals(oldValue, newValue))
                        field.SetValue(newValue);
                }
                else if (field.Type == SerializedPropertyType.ObjectReference) {
                    var oldValue = (Object) field.GetValue();
                    var newValue =
                        EditorGUILayout.ObjectField(field.Name, oldValue, field.Info.PropertyType, false, emptyOptions);
                    if (oldValue != newValue)
                        field.SetValue(newValue);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        public static PropertyField[] GetProperties(object obj) {
            var fields = new List<PropertyField>();

            var infos = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var info in infos) {
                if (!(info.CanRead && info.CanWrite))
                    continue;

                var attributes = info.GetCustomAttributes(true);

                var isExposed = false;
                foreach (var o in attributes)
                    if (o.GetType() == typeof(ExposePropertyAttribute)) {
                        isExposed = true;
                        break;
                    }

                if (!isExposed)
                    continue;

                SerializedPropertyType type;
                if (PropertyField.GetPropertyType(info, out type)) {
                    var field = new PropertyField(obj, info, type);
                    fields.Add(field);
                }
            }

            return fields.ToArray();
        }
    }

    public class PropertyField {
        private readonly MethodInfo _getter;
        private readonly PropertyInfo _info;
        private readonly object _obj;
        private readonly MethodInfo _setter;
        private readonly SerializedPropertyType _type;

        public PropertyField(object obj, PropertyInfo info, SerializedPropertyType type) {
            _obj = obj;
            _info = info;
            _type = type;

            _getter = _info.GetGetMethod();
            _setter = _info.GetSetMethod();
        }

        public PropertyInfo Info {
            get { return _info; }
        }

        public SerializedPropertyType Type {
            get { return _type; }
        }

        public string Name {
            get { return ObjectNames.NicifyVariableName(_info.Name); }
        }

        public object GetValue() {
            return _getter.Invoke(_obj, null);
        }

        public void SetValue(object value) {
            _setter.Invoke(_obj, new[] {value});
        }

        public static bool GetPropertyType(PropertyInfo info, out SerializedPropertyType propertyType) {
            var type = info.PropertyType;
            propertyType = SerializedPropertyType.Generic;
            if (type == typeof(int))
                propertyType = SerializedPropertyType.Integer;
            else if (type == typeof(float))
                propertyType = SerializedPropertyType.Float;
            else if (type == typeof(bool))
                propertyType = SerializedPropertyType.Boolean;
            else if (type == typeof(string))
                propertyType = SerializedPropertyType.String;
            else if (type == typeof(Vector2))
                propertyType = SerializedPropertyType.Vector2;
            else if (type == typeof(Vector3))
                propertyType = SerializedPropertyType.Vector3;
            else if (type.IsEnum)
                propertyType = SerializedPropertyType.Enum;
            else if (typeof(MonoBehaviour).IsAssignableFrom(type))
                propertyType = SerializedPropertyType.ObjectReference;
            return propertyType != SerializedPropertyType.Generic;
        }
    }
}