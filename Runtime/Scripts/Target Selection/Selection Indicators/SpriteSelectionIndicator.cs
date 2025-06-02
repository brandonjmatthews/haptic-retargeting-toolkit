/*
 * HRTK: SpriteSelectionIndicator.cs
 *
 * Copyright (c) 2019 Brandon Matthews
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HRTK
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteSelectionIndicator : SelectionIndicator
    {
        [SerializeField] Sprite selected;
        [SerializeField] Sprite deselected;
        SpriteRenderer spriteRenderer;
        Color defaultColor = Color.white;

        public override void Initalize()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = deselected;
            defaultColor = spriteRenderer.color;
        }

        public override void OnDeselected()
        {

            spriteRenderer.sprite = deselected;
            spriteRenderer.color = defaultColor;
        }

        public override void OnSelected()
        {
            spriteRenderer.sprite = selected;
            spriteRenderer.color = defaultColor;
        }

        public override void OnSelected(Color selectedColor)
        {
            spriteRenderer.sprite = selected;
            spriteRenderer.color = selectedColor;
        }
    }
}

