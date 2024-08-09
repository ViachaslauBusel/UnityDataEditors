using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ObjectRegistryEditor.Helpers
{
    internal static class GUIHelper
    {
        internal static int DrawPagesSelector(int currentPage, int totalPages)
        {
            GUILayout.BeginHorizontal(GUIStyle.none, GUILayout.Width(80));

            if (GUILayout.Button("", RedactorStyle.Left, GUILayout.Height(20), GUILayout.Width(20)) && currentPage > 0)
            {
                currentPage--;
            }
            GUILayout.Space(5.0f);
            GUILayout.Label((currentPage + 1) + "/" + (totalPages + 1), RedactorStyle.Text);
            GUILayout.Space(5.0f);
            if (GUILayout.Button("", RedactorStyle.Right, GUILayout.Height(20), GUILayout.Width(20)) && currentPage < totalPages)
            {
                currentPage++;
            }

            GUILayout.EndHorizontal();

            return currentPage;
        }
    }
}
