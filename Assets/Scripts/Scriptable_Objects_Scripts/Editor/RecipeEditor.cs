using Data;
using Scriptable_Objects_Scripts;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(Recipe))]
public class RecipeEditor : Editor
{
  // ── Layout constants ──────────────────────────────────────────────────
  private const float SlotSize = 68f;
  private const float SlotPad = 6f;

  // ── Colours ───────────────────────────────────────────────────────────
  private static readonly Color ColSlotEmpty = new(0.18f, 0.18f, 0.18f, 1f);
  private static readonly Color ColSlotFilled = new(0.22f, 0.35f, 0.22f, 1f);
  private static readonly Color ColSlotDisabled = new(0.12f, 0.12f, 0.12f, 1f);
  private static readonly Color ColBorderEmpty = new(0.35f, 0.35f, 0.35f, 1f);
  private static readonly Color ColBorderFilled = new(0.45f, 0.75f, 0.45f, 1f);
  private static readonly Color ColBorderDisabled = new(0.22f, 0.22f, 0.22f, 1f);
  private static readonly Color ColHover = new(1f, 1f, 1f, 0.07f);
  private static readonly Color ColResultBg = new(0.15f, 0.20f, 0.30f, 1f);
  private static readonly Color ColResultBorder = new(0.40f, 0.55f, 0.90f, 1f);

  // ── Serialised properties ─────────────────────────────────────────────
  private SerializedProperty _result;
  private SerializedProperty _amount;
  private SerializedProperty _gridSize;
  private SerializedProperty _ingredients;

  // ── Picker tracking ───────────────────────────────────────────────────
  private int _activePickerSlot = -1; // -2 = result picker

  // ─────────────────────────────────────────────────────────────────────
  private void OnEnable()
  {
    _result = serializedObject.FindProperty("result");
    _amount = serializedObject.FindProperty("amount");
    _gridSize = serializedObject.FindProperty("gridSize");
    _ingredients = serializedObject.FindProperty("ingredients");
  }

  // ─────────────────────────────────────────────────────────────────────
  public override void OnInspectorGUI()
  {
    serializedObject.Update();

    DrawOutputSection();
    GUILayout.Space(14f);
    DrawGridSizeSelector();
    GUILayout.Space(8f);
    DrawGridSection();
    GUILayout.Space(10f);
    DrawFooterButtons();

    serializedObject.ApplyModifiedProperties();

    if (_activePickerSlot != -1)
      Repaint();
  }

  // ── Output ────────────────────────────────────────────────────────────
  private void DrawOutputSection()
  {
    GUILayout.Label("Recipe Output", EditorStyles.boldLabel);

    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

    ItemData resultItem = _result.objectReferenceValue as ItemData;
    Rect resultSlotRect = GUILayoutUtility.GetRect(SlotSize + 14, SlotSize + 14,
                               GUILayout.Width(SlotSize + 14), GUILayout.Height(SlotSize + 14));

    DrawSlotBackground(resultSlotRect, resultItem != null, ColResultBg, ColResultBorder);
    DrawSlotContent(resultSlotRect, resultItem);
    HandleSlotInteraction(resultSlotRect, _result, -2);

    EditorGUILayout.BeginVertical();
    GUILayout.Space(6);
    EditorGUILayout.LabelField("Result Item", EditorStyles.miniLabel);
    EditorGUILayout.PropertyField(_result, GUIContent.none);
    GUILayout.Space(6);
    EditorGUILayout.LabelField("Output Amount", EditorStyles.miniLabel);
    _amount.intValue = Mathf.Max(1, EditorGUILayout.IntField(_amount.intValue));
    EditorGUILayout.EndVertical();

    EditorGUILayout.EndHorizontal();
  }

  // ── Grid size selector ────────────────────────────────────────────────
  private void DrawGridSizeSelector()
  {
    EditorGUILayout.BeginHorizontal();
    GUILayout.Label("Grid Size", EditorStyles.boldLabel, GUILayout.Width(70));

    int[] sizes = { 2, 3, 4 };

    foreach (int size in sizes)
    {
      bool selected = _gridSize.intValue == size;
      GUI.backgroundColor = selected ? new Color(0.3f, 0.7f, 0.3f) : Color.white;

      if (GUILayout.Button($"{size}×{size}", GUILayout.Width(50), GUILayout.Height(22)) && !selected)
      {
        ClearOutOfBoundsSlots(size);
        _gridSize.intValue = size;
      }
    }

    GUI.backgroundColor = Color.white;
    EditorGUILayout.EndHorizontal();
  }

  // ── Grid ──────────────────────────────────────────────────────────────
  private void DrawGridSection()
  {
    int gridSize = _gridSize.intValue;
    GUILayout.Label($"Ingredients  ({gridSize} × {gridSize})", EditorStyles.boldLabel);
    GUILayout.Space(4f);

    // Always render a 4×4 frame; slots outside the active grid are dimmed and non-interactive
    float gridWidth = 4 * SlotSize + 3 * SlotPad;

    EditorGUILayout.BeginHorizontal();
    GUILayout.FlexibleSpace();
    EditorGUILayout.BeginVertical(GUILayout.Width(gridWidth));

    for (int row = 0; row < 4; row++)
    {
      EditorGUILayout.BeginHorizontal();
      for (int col = 0; col < 4; col++)
      {
        int index = row * 4 + col;
        bool inBounds = row < gridSize && col < gridSize;

        Rect rect = GUILayoutUtility.GetRect(SlotSize, SlotSize,
                     GUILayout.Width(SlotSize), GUILayout.Height(SlotSize));

        if (inBounds)
        {
          SerializedProperty slot = _ingredients.GetArrayElementAtIndex(index);
          ItemData item = slot.objectReferenceValue as ItemData;

          DrawSlotBackground(rect, item != null, ColSlotEmpty, ColBorderEmpty,
                                                 ColSlotFilled, ColBorderFilled);
          DrawSlotContent(rect, item);
          HandleSlotInteraction(rect, slot, index);
        }
        else
        {
          DrawSlotBackground(rect, false, ColSlotDisabled, ColBorderDisabled);
        }

        if (col < 3) GUILayout.Space(SlotPad);
      }
      EditorGUILayout.EndHorizontal();
      if (row < 3) GUILayout.Space(SlotPad);
    }

    EditorGUILayout.EndVertical();
    GUILayout.FlexibleSpace();
    EditorGUILayout.EndHorizontal();
  }

  // ── Footer ────────────────────────────────────────────────────────────
  private void DrawFooterButtons()
  {
    EditorGUILayout.BeginHorizontal();
    GUILayout.FlexibleSpace();

    GUI.backgroundColor = new Color(0.8f, 0.3f, 0.3f);
    if (GUILayout.Button("Clear All Slots", GUILayout.Width(130), GUILayout.Height(24)))
    {
      if (EditorUtility.DisplayDialog("Clear Recipe",
          "Remove all ingredients from the grid?", "Clear", "Cancel"))
      {
        for (int i = 0; i < 16; i++)
          _ingredients.GetArrayElementAtIndex(i).objectReferenceValue = null;
      }
    }
    GUI.backgroundColor = Color.white;
    EditorGUILayout.EndHorizontal();
  }

  // ── Clears slots outside the new grid bounds before switching size ────
  private void ClearOutOfBoundsSlots(int newSize)
  {
    for (int row = 0; row < 4; row++)
      for (int col = 0; col < 4; col++)
        if (row >= newSize || col >= newSize)
          _ingredients.GetArrayElementAtIndex(row * 4 + col).objectReferenceValue = null;

    serializedObject.ApplyModifiedProperties();
  }

  // ─────────────────────────────────────────────────────────────────────
  //  Drawing helpers
  // ─────────────────────────────────────────────────────────────────────
  private static void DrawSlotBackground(
      Rect rect, bool filled,
      Color emptyBg, Color emptyBorder,
      Color? filledBg = null, Color? filledBorder = null)
  {
    Color bg = filled ? (filledBg ?? ColSlotFilled) : emptyBg;
    Color border = filled ? (filledBorder ?? ColBorderFilled) : emptyBorder;

    EditorGUI.DrawRect(rect, bg);
    DrawBorder(rect, 1.5f, border);
  }

  private static void DrawSlotContent(Rect rect, ItemData item)
  {
    if (rect.Contains(Event.current.mousePosition))
      EditorGUI.DrawRect(rect, ColHover);

    if (item == null)
    {
      GUI.Label(rect, "+", new GUIStyle(EditorStyles.label)
      {
        alignment = TextAnchor.MiddleCenter,
        fontSize = 22,
        normal = { textColor = new Color(0.45f, 0.45f, 0.45f) }
      });
      return;
    }

    GUI.Label(rect, item.name, new GUIStyle(EditorStyles.miniLabel)
    {
      alignment = TextAnchor.MiddleCenter,
      wordWrap = true,
      normal = { textColor = Color.white }
    });
  }

  private static void DrawBorder(Rect r, float t, Color c)
  {
    EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, t), c);
    EditorGUI.DrawRect(new Rect(r.x, r.yMax - t, r.width, t), c);
    EditorGUI.DrawRect(new Rect(r.x, r.y, t, r.height), c);
    EditorGUI.DrawRect(new Rect(r.xMax - t, r.y, t, r.height), c);
  }

  // ─────────────────────────────────────────────────────────────────────
  //  Interaction helpers
  // ─────────────────────────────────────────────────────────────────────
  private void HandleSlotInteraction(Rect rect, SerializedProperty slot, int slotIndex)
  {
    Event e = Event.current;

    // ── Drag & Drop ───────────────────────────────────────────────────
    if (rect.Contains(e.mousePosition))
    {
      if (e.type == EventType.DragUpdated)
      {
        DragAndDrop.visualMode = IsValidDrag() ? DragAndDropVisualMode.Copy
                                               : DragAndDropVisualMode.Rejected;
        e.Use();
      }
      else if (e.type == EventType.DragPerform && IsValidDrag())
      {
        DragAndDrop.AcceptDrag();
        foreach (Object obj in DragAndDrop.objectReferences)
        {
          if (obj is ItemData id)
          {
            slot.objectReferenceValue = id;
            serializedObject.ApplyModifiedProperties();
            break;
          }
        }
        e.Use();
      }
    }

    // ── Left-click → object picker ────────────────────────────────────
    if (e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition))
    {
      _activePickerSlot = slotIndex;
      EditorGUIUtility.ShowObjectPicker<ItemData>(
          slot.objectReferenceValue as ItemData, false, "", slotIndex);
      e.Use();
    }

    // ── Receive picker result ─────────────────────────────────────────
    if (e.commandName == "ObjectSelectorUpdated" &&
        EditorGUIUtility.GetObjectPickerControlID() == slotIndex)
    {
      slot.objectReferenceValue = EditorGUIUtility.GetObjectPickerObject() as ItemData;
      serializedObject.ApplyModifiedProperties();
    }

    if (e.commandName == "ObjectSelectorClosed" &&
        EditorGUIUtility.GetObjectPickerControlID() == slotIndex)
      _activePickerSlot = -1;

    // ── Right-click context menu ──────────────────────────────────────
    if (e.type == EventType.ContextClick && rect.Contains(e.mousePosition))
    {
      GenericMenu menu = new();

      if (slot.objectReferenceValue != null)
      {
        SerializedProperty captured = slot;
        menu.AddItem(new GUIContent("Clear Slot"), false, () =>
        {
          captured.objectReferenceValue = null;
          serializedObject.ApplyModifiedProperties();
          Repaint();
        });
        menu.AddItem(new GUIContent("Ping Asset"), false, () =>
            EditorGUIUtility.PingObject(slot.objectReferenceValue));
      }
      else
      {
        menu.AddDisabledItem(new GUIContent("Slot is empty"));
      }
      menu.ShowAsContext();
      e.Use();
    }
  }

  private static bool IsValidDrag()
  {
    foreach (Object obj in DragAndDrop.objectReferences)
      if (obj is ItemData) return true;
    return false;
  }
}
