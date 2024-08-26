using Codice.Client.BaseCommands;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

enum SmoothnessSource
{
    Uniform, Albedo, Metallic
}

public class MyLightingShaderGUI : ShaderGUI
{

    static GUIContent staticLabel = new GUIContent();
    // static ColorPickerHDRConfig emissionConfig = new ColorPickerHDRConfig(0f, 99f, 1f / 99f, 3f);

    Material target;
    MaterialEditor editor;
    MaterialProperty[] properties;

    static GUIContent MakeLabel(string text, string tooltip = null)
    {
        staticLabel.text = text;
        staticLabel.tooltip = tooltip;
        return staticLabel;
    }

    static GUIContent MakeLabel(MaterialProperty property, string tooltip = null)
    {
        staticLabel.text = property.displayName;
        staticLabel.tooltip = tooltip;
        return staticLabel;
    }

    public override void OnGUI(MaterialEditor editor, MaterialProperty[] properties)
    {
        this.target = editor.target as Material;
        this.editor = editor;
        this.properties = properties;
        DoMain();
        DoSecondary();
    }

    void DoMain()
    {
        GUILayout.Label("Main Maps", EditorStyles.boldLabel);

        MaterialProperty mainTex = FindProperty("_MainTex");
        editor.TexturePropertySingleLine(MakeLabel(mainTex, "Albedo (RGB)"), mainTex, FindProperty("_Tint"));
        DoMetallic();
        DoSmoothness();
        DoOcclusion();
        DoEmission();
        DoDetailMask();
        DoNormals();
        editor.TextureScaleOffsetProperty(mainTex);
    }

    void DoMetallic()
    {
        MaterialProperty map = FindProperty("_MetallicMap");
        EditorGUI.BeginChangeCheck();
        editor.TexturePropertySingleLine(MakeLabel(map, "Metallic (R)"), map, map.textureValue ? null : FindProperty("_Metallic"));
        if (EditorGUI.EndChangeCheck())
        {
            SetKeyword("_METALLIC_MAP", map.textureValue);
        }
    }

    void DoSmoothness()
    {
		SmoothnessSource source = SmoothnessSource.Uniform;
		if (IsKeywordEnabled("_SMOOTHNESS_ALBEDO")) {
			source = SmoothnessSource.Albedo;
		}
		else if (IsKeywordEnabled("_SMOOTHNESS_METALLIC")) {
			source = SmoothnessSource.Metallic;
		}
        MaterialProperty slider = FindProperty("_Smoothness");
        EditorGUI.indentLevel += 2;
        editor.ShaderProperty(slider, MakeLabel(slider));
        EditorGUI.indentLevel += 1;
        EditorGUI.BeginChangeCheck();
        RecordAction("Smoothness Source");
        source = (SmoothnessSource)EditorGUILayout.EnumPopup(MakeLabel("Source"), source);
        if (EditorGUI.EndChangeCheck())
        {
            SetKeyword("_SMOOTHNESS_ALBEDO", source == SmoothnessSource.Albedo);
            SetKeyword("_SMOOTHNESS_METALLIC", source == SmoothnessSource.Metallic);
        }
        EditorGUI.indentLevel -= 3;
    }

    void DoOcclusion()
    {
        MaterialProperty map = FindProperty("_OcclusionMap");
        EditorGUI.BeginChangeCheck();
        editor.TexturePropertySingleLine(
            MakeLabel(map, "Occlusion (G)"), map,
            map.textureValue ? FindProperty("_OcclusionStrength") : null
        );
        if (EditorGUI.EndChangeCheck())
        {
            SetKeyword("_OCCLUSION_MAP", map.textureValue);
        }
    }

    void DoEmission()
    {
        MaterialProperty map = FindProperty("_EmissionMap");
        EditorGUI.BeginChangeCheck();
        editor.TexturePropertyWithHDRColor(MakeLabel("Emission (RGB)"), map, FindProperty("_Emission"), false);
        if (EditorGUI.EndChangeCheck())
        {
            SetKeyword("_EMISSION_MAP", map.textureValue);
        }
    }

    void DoDetailMask()
    {
        MaterialProperty mask = FindProperty("_DetailMask");
        EditorGUI.BeginChangeCheck();
        editor.TexturePropertySingleLine(
            MakeLabel(mask, "Detail Mask (A)"), mask
        );
        if (EditorGUI.EndChangeCheck())
        {
            SetKeyword("_DETAIL_MASK", mask.textureValue);
        }
    }

    void DoNormals()
    {
        MaterialProperty map = FindProperty("_NormalMap");
        editor.TexturePropertySingleLine(MakeLabel(map), map, map.textureValue ? FindProperty("_BumpScale") : null);
    }

    void DoSecondary()
    {
        GUILayout.Label("Secondary Maps", EditorStyles.boldLabel);

        MaterialProperty detailTex = FindProperty("_DetailTex");
        editor.TexturePropertySingleLine(MakeLabel(detailTex, "Albedo (RGB) multiplied by 2"), detailTex);
        DoSecondaryNormals();
        editor.TextureScaleOffsetProperty(detailTex);
    }

    void DoSecondaryNormals()
    {
        MaterialProperty map = FindProperty("_DetailNormalMap");
        editor.TexturePropertySingleLine(MakeLabel(map), map, map.textureValue ? FindProperty("_DetailBumpScale") : null);
    }

    MaterialProperty FindProperty(string name)
    {
        return FindProperty(name, properties);
    }

    void SetKeyword(string keyword, bool state)
    {
        if (state)
        {
            target.EnableKeyword(keyword);
        } else
        {
            target.DisableKeyword(keyword);
        }
    }

    bool IsKeywordEnabled(string keyword)
    {
        return target.IsKeywordEnabled(keyword);
    }
    void RecordAction(string label)
    {
        editor.RegisterPropertyChangeUndo(label);
    }

}
