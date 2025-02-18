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

            GUILayout.BeginVertical();
            GUILayout.Space(2.0f); // Add space to lower the label
            if (GUILayout.Button("", RedactorStyle.Left, GUILayout.Height(20), GUILayout.Width(20)) && currentPage > 0)
            {
                currentPage--;
            }
            GUILayout.EndVertical();
            GUILayout.Space(5.0f);

            // Convert currentPage to string for the text field
            string pageText = (currentPage + 1).ToString();
            pageText = GUILayout.TextField(pageText, GUILayout.Width(30));

            // Try to parse the input and update currentPage
            if (int.TryParse(pageText, out int newPage))
            {
                currentPage = Mathf.Clamp(newPage - 1, 0, totalPages);
            }

            GUILayout.BeginVertical();
            GUILayout.Space(2.0f); // Add space to lower the label
            GUILayout.Label("/ " + (totalPages + 1), RedactorStyle.Text);
            GUILayout.EndVertical();
            GUILayout.Space(5.0f);

            GUILayout.BeginVertical();
            GUILayout.Space(2.0f); // Add space to lower the label
            if (GUILayout.Button("", RedactorStyle.Right, GUILayout.Height(20), GUILayout.Width(20)) && currentPage < totalPages)
            {
                currentPage++;
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            return currentPage;
        }
    }
}
