using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LPGD.TextureUtilities
{
    [CreateAssetMenu(menuName = "LPGD/TextureUtilities/PaletteTextureCreator")]
    public class PaletteTextureCreator : ScriptableObject
    {
        [SerializeField] string m_SourcePath;
        [SerializeField] string m_OutputPath;

        [ContextMenu("GenerateTextureWithDataFromPath")]
        public void GenerateTextureWithDataFromPath()
        {
            try
            {
                List<Color> colors = null;
                byte[] bytes;
                using (System.IO.FileStream fsSource = new System.IO.FileStream(m_SourcePath,
                                                           System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    // Read the source file into a byte array.
                    bytes = new byte[fsSource.Length];
                    int numBytesToRead = (int)fsSource.Length;
                    int numBytesRead = 0;
                    string result = null;
                    while (numBytesToRead > 0)
                    {
                        // Read may return anything from 0 to numBytesToRead.
                        int n = fsSource.Read(bytes, numBytesRead, numBytesToRead);

                        // Break when the end of the file is reached.
                        if (n == 0)
                            break;

                        numBytesRead += n;
                        numBytesToRead -= n;

                        result = System.Text.Encoding.UTF8.GetString(bytes);
                    }
                    numBytesToRead = bytes.Length;

                    string[] colorValues = result.Split('\n');

                    colors = new List<Color>(colorValues.Length);
                    for (int i = 0; i < colorValues.Length; i++)
                    {
                        colorValues[i] = colorValues[i].Replace("\r", string.Empty);

                        if (string.IsNullOrWhiteSpace(colorValues[i]))
                            continue;

                        if (colorValues[i].Length != 6)
                            continue;

                        int[] rgb = new int[3];
                        for (int j = 0; j < 6; j += 2)
                        {
                            string value = colorValues[i].Substring(j, 2);
                            int intValue = int.Parse(value, System.Globalization.NumberStyles.HexNumber);

                            rgb[j / 2] = intValue;
                        }

                        colors.Add(new Color(rgb[0] / 255f, rgb[1] / 255f, rgb[2] / 255f));
                    }
                }
                if (colors != null)
                {
                    for (int i = 0; i < colors.Count; i++)
                    {
                        Debug.Log(colors[i].r + " :: " + colors[i].g + " :: " + colors[i].b);
                    }
                }

                int nearestSquareRoot = (int)Mathf.Ceil(Mathf.Sqrt(colors.Count));

                int delta = (nearestSquareRoot * nearestSquareRoot) - colors.Count;
                if (delta > 0)
                {
                    for (int i = 0; i < delta; i++)
                    {
                        colors.Add(Color.black);
                    }
                }

                Texture2D tex = new Texture2D(nearestSquareRoot, nearestSquareRoot);
                tex.SetPixels(colors.ToArray());
                tex.Apply();

                bytes = tex.EncodeToPNG();
                System.IO.FileStream stream = new System.IO.FileStream(m_OutputPath + "Palette.png", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
                System.IO.BinaryWriter writer = new System.IO.BinaryWriter(stream);
                for (int i = 0; i < bytes.Length; i++)
                {
                    writer.Write(bytes[i]);
                }
                writer.Close();
                stream.Close();
                DestroyImmediate(tex);
                //I can't figure out how to import the newly created .png file as a texture
#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
#endif
            }
            catch (System.IO.FileNotFoundException ioEx)
            {
                Debug.Log(ioEx.Message);
            }
        }
    }
}