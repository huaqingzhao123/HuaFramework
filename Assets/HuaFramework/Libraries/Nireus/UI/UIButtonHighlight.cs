using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nireus
{

    public class UIButtonHighlight : MonoBehaviour
    {
        
        public ColorBlock color_block_backup;
        public Color color_highlight_color;

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            color_block_backup = button.colors;
            color_highlight_color = color_block_backup.highlightedColor;
        }

        public void ChangeHighlightColor(Color c)
        {
            color_highlight_color = c;
        }

        public void ChangeHighlightColor(Color32 c)
        {
            color_highlight_color = c;
        }

        public void SetHighlightToNomalState(bool select)
        {
            if (select)
            {
                ColorBlock colors = color_block_backup;
                colors.normalColor = color_highlight_color;
                button.colors = colors;
            }
            else
            {
                button.colors = color_block_backup;
            }
        }
    }
}