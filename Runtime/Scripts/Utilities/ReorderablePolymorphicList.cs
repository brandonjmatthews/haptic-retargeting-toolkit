// Adapted from: ChristianQui's reorderable polymorphic list.
// MIT License
// Copyright (c) 2020 Cristian Qiu Félez
// https://github.com/CristianQiu/Unity-Editor-PolymorphicReorderableList/blob/master/Assets/Code/Editor/BaseCharacterEditor.cs
// Modifications (c) 2023 Brandon Matthews

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;

public class ReorderablePolymorphicList : ReorderableList
{
    SerializedObject serializedObject;
    GUIStyle headersStyle;

    private const float AdditionalSpaceMultiplier = 1.0f;

    private const float HeightHeader = 20.0f;
    private const float MarginReorderIcon = 20.0f;
    private const float ShrinkHeaderWidth = 15.0f;
    private const float XShiftHeaders = 15.0f;
    private static readonly Color ProSkinTextColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
    private static readonly Color PersonalSkinTextColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);

    private static readonly Color ProSkinSelectionBgColor = new Color(44.0f / 255.0f, 93.0f / 255.0f, 135.0f / 255.0f, 1.0f);
    private static readonly Color PersonalSkinSelectionBgColor = new Color(58.0f / 255.0f, 114.0f / 255.0f, 176.0f / 255.0f, 1.0f);


    public ReorderablePolymorphicList(SerializedObject serializedObject, SerializedProperty elements)
        : this(serializedObject, elements, true, true, true, true)
    {

    }

    public ReorderablePolymorphicList(SerializedObject serializedObject, SerializedProperty elements, bool draggable, bool displayHeader, bool displayAddButton, bool displayRemoveButton)
        : base(serializedObject, elements, draggable, displayHeader, displayAddButton, displayRemoveButton)
    {
        this.serializedObject = serializedObject;

        headersStyle = new GUIStyle();
        headersStyle.alignment = TextAnchor.MiddleLeft;
        headersStyle.normal.textColor = EditorGUIUtility.isProSkin ? ProSkinTextColor : PersonalSkinTextColor;
        headersStyle.fontStyle = FontStyle.Bold;

        this.drawHeaderCallback += OnDrawReorderListHeader;
        this.drawElementCallback += OnDrawReorderListElement;
        this.drawElementBackgroundCallback += OnDrawReorderListBg;
        this.elementHeightCallback += OnReorderListElementHeight;
        this.onAddDropdownCallback += OnReorderListAddDropdown;
    }

    #region ReorderableList Callbacks

    private void OnDrawReorderListHeader(Rect rect)
    {
        EditorGUI.LabelField(rect, serializedProperty.displayName);
    }

    private void OnDrawReorderListElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        int length = serializedProperty.arraySize;

        if (length <= 0)
            return;

        SerializedProperty iteratorProp = serializedProperty.GetArrayElementAtIndex(index);

        Rect labelfoldRect = rect;
        labelfoldRect.height = HeightHeader;
        labelfoldRect.x += XShiftHeaders;
        labelfoldRect.width -= ShrinkHeaderWidth;

        string[] splitReferenceName = iteratorProp.managedReferenceFullTypename.Split('.');
        string propName = splitReferenceName[splitReferenceName.Length - 1];
        propName = UnityEditor.ObjectNames.NicifyVariableName(propName);


        iteratorProp.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(labelfoldRect, iteratorProp.isExpanded, propName);

        if (iteratorProp.isExpanded)
        {
            ++EditorGUI.indentLevel;

            SerializedProperty endProp = iteratorProp.GetEndProperty();

            int i = 0;
            while (iteratorProp.NextVisible(true) && !EqualContents(endProp, iteratorProp))
            {
                float multiplier = i == 0 ? AdditionalSpaceMultiplier : 1.0f;
                rect.y += GetDefaultSpaceBetweenElements() * multiplier;
                rect.height = EditorGUIUtility.singleLineHeight;

                EditorGUI.PropertyField(rect, iteratorProp, false);
   
                ++i;
            }

            --EditorGUI.indentLevel;
        }

        EditorGUI.EndFoldoutHeaderGroup();
        // var item = serializedProperty.GetArrayElementAtIndex(index);
        // EditorGUI.PropertyField(rect, item);

    }

    private void OnDrawReorderListBg(Rect rect, int index, bool isActive, bool isFocused)
    {
        if (!isFocused || !isActive)
            return;

        float height = OnReorderListElementHeight(index);

        SerializedProperty prop = serializedProperty.GetArrayElementAtIndex(index);

        // remove a bit of the line that goes beyond the header label
        if (!prop.isExpanded)
            height -= EditorGUIUtility.standardVerticalSpacing;

        Rect copyRect = rect;
        copyRect.width = MarginReorderIcon;
        copyRect.height = height;

        // draw two rects indepently to avoid overlapping the header label
        Color color = EditorGUIUtility.isProSkin ? ProSkinSelectionBgColor : PersonalSkinSelectionBgColor;
        EditorGUI.DrawRect(copyRect, color);

        float offset = 2.0f;
        rect.x += MarginReorderIcon;
        rect.width -= (MarginReorderIcon + offset);

        rect.height = height - HeightHeader + offset;
        rect.y += HeightHeader - offset;

        EditorGUI.DrawRect(rect, color);
    }

    private float OnReorderListElementHeight(int index)
    {
        int length = serializedProperty.arraySize;

        if (length <= 0)
            return 0.0f;

        SerializedProperty iteratorProp = serializedProperty.GetArrayElementAtIndex(index);
        SerializedProperty endProp = iteratorProp.GetEndProperty();

        float height = GetDefaultSpaceBetweenElements();

        if (!iteratorProp.isExpanded)
            return height;

        int i = 0;
        while (iteratorProp.NextVisible(true) && !EqualContents(endProp, iteratorProp))
        {
            float multiplier = i == 0 ? AdditionalSpaceMultiplier : 1.0f;
            height += GetDefaultSpaceBetweenElements() * multiplier;
            ++i;
        }

        return height;
    }

    private void OnReorderListAddDropdown(Rect buttonRect, ReorderableList list)
    {
        GenericMenu menu = new GenericMenu();

        List<Type> showTypes = GetNonAbstractTypesSubclassOf(GetListPropertyGenericType(serializedProperty));

        for (int i = 0; i < showTypes.Count; ++i)
        {
            Type type = showTypes[i];
            string actionName = showTypes[i].Name;

            // UX improvement: If no elements are available the add button should be faded out or
            // just not visible.
            // bool alreadyHasIt = DoesReordListHaveElementOfType(actionName);
            // if (alreadyHasIt)
            //     continue;

            InsertSpaceBeforeCaps(ref actionName);
            menu.AddItem(new GUIContent(actionName), false, OnAddItemFromDropdown, (object)type);
        }

        menu.ShowAsContext();
    }

    private void OnAddItemFromDropdown(object obj)
    {
        Type settingsType = (Type)obj;

        Debug.Log(settingsType.Name);

        int last = serializedProperty.arraySize;
        serializedProperty.InsertArrayElementAtIndex(last);

        SerializedProperty lastProp = serializedProperty.GetArrayElementAtIndex(last);
        Debug.Log(lastProp.propertyType);
        lastProp.managedReferenceValue = Activator.CreateInstance(settingsType);


        serializedObject.ApplyModifiedProperties();
    }

    #endregion

    #region Helper Methods

    public static System.Type GetListPropertyGenericType(SerializedProperty property)
    {
        System.Type parentType = property.serializedObject.targetObject.GetType();
        System.Reflection.FieldInfo fi = parentType.GetField(property.propertyPath);
        return fi.FieldType.GetGenericArguments()[0];
    }

    public static System.Type GetPropertyType(SerializedProperty property)
    {
        System.Type parentType = property.serializedObject.targetObject.GetType();
        System.Reflection.FieldInfo fi = parentType.GetField(property.propertyPath);
        return fi.FieldType;
    }
    
    private float GetDefaultSpaceBetweenElements()
    {
        return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
    }

    private void InsertSpaceBeforeCaps(ref string theString)
    {
        for (int i = 0; i < theString.Length; ++i)
        {
            char currChar = theString[i];

            if (char.IsUpper(currChar))
            {
                theString = theString.Insert(i, " ");
                ++i;
            }
        }
    }

    private bool EqualContents(SerializedProperty a, SerializedProperty b)
    {
        return SerializedProperty.EqualContents(a, b);
    }

    private List<Type> GetNonAbstractTypesSubclassOf(Type parentType, bool sorted = true)
    {
        Assembly assembly = Assembly.GetAssembly(parentType);

        List<Type> types = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(parentType)).ToList();

        if (sorted)
            types.Sort(CompareTypesNames);

        if (types.Count == 0)
            types.Add(parentType);

        return types;
    }

    private int CompareTypesNames(Type a, Type b)
    {
        return a.Name.CompareTo(b.Name);
    }

    private bool DoesReordListHaveElementOfType(string type)
    {
        for (int i = 0; i < serializedProperty.arraySize; ++i)
        {
            // this works but feels ugly. Type in the array element looks like "managedReference<actualStringType>"
            if (serializedProperty.GetArrayElementAtIndex(i).type.Contains(type))
                return true;
        }

        return false;
    }
    #endregion
}

#endif