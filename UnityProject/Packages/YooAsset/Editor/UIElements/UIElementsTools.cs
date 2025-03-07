﻿#if UNITY_2019_4_OR_NEWER
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    public static class UIElementsTools
    {
        /// <summary>
        /// 设置元素显隐
        /// </summary>
        public static void SetElementVisible(VisualElement element, bool visible)
        {
            if (element == null)
                return;

            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            element.style.visibility = visible ? Visibility.Visible : Visibility.Hidden;
        }

        /// <summary>
        /// 设置元素的文本最小宽度
        /// </summary>
        public static void SetElementLabelMinWidth(VisualElement element, int minWidth)
        {
            var label = element.Q<Label>();
            if (label != null)
            {
                // 设置最小宽度
                label.style.minWidth = minWidth;
            }
        }

        /// <summary>
        /// 设置按钮图标
        /// </summary>
        public static void SetToolbarButtonIcon(ToolbarButton element, string iconName)
        {
            var image = EditorGUIUtility.IconContent(iconName).image as Texture2D;
            element.style.backgroundImage = image;
            element.text = string.Empty;
        }

        /// <summary>
        /// 竖版分屏
        /// </summary>
        public static void SplitVerticalPanel(VisualElement root, VisualElement panelA, VisualElement panelB)
        {
#if UNITY_2020_3_OR_NEWER
            root.Remove(panelA);
            root.Remove(panelB);

            var spliteView = new TwoPaneSplitView();
            spliteView.fixedPaneInitialDimension = 300;
            spliteView.orientation = TwoPaneSplitViewOrientation.Vertical;
            spliteView.contentContainer.Add(panelA);
            spliteView.contentContainer.Add(panelB);
            root.Add(spliteView);
#endif
        }
    }
}
#endif