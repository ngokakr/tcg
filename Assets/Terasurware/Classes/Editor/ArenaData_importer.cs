using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;
using System.Xml.Serialization;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

public class ArenaData_importer : AssetPostprocessor {
	private static readonly string filePath = "Assets/ExcelData/ArenaData.xls";
	private static readonly string exportPath = "Assets/ExcelData/ArenaData.asset";
	private static readonly string[] sheetNames = { "Sheet1", };
	
	static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		foreach (string asset in importedAssets) {
			if (!filePath.Equals (asset))
				continue;
				
			XLS_ArenaData data = (XLS_ArenaData)AssetDatabase.LoadAssetAtPath (exportPath, typeof(XLS_ArenaData));
			if (data == null) {
				data = ScriptableObject.CreateInstance<XLS_ArenaData> ();
				AssetDatabase.CreateAsset ((ScriptableObject)data, exportPath);
				data.hideFlags = HideFlags.NotEditable;
			}
			
			data.sheets.Clear ();
			using (FileStream stream = File.Open (filePath, FileMode.Open, FileAccess.Read)) {
				IWorkbook book = new HSSFWorkbook (stream);
				
				foreach(string sheetName in sheetNames) {
					ISheet sheet = book.GetSheet(sheetName);
					if( sheet == null ) {
						Debug.LogError("[QuestData] sheet not found:" + sheetName);
						continue;
					}

					XLS_ArenaData.Sheet s = new XLS_ArenaData.Sheet ();
					s.name = sheetName;
				
					for (int i=1; i<= sheet.LastRowNum; i++) {
						IRow row = sheet.GetRow (i);
						ICell cell = null;
						
						XLS_ArenaData.Param p = new XLS_ArenaData.Param ();
						
					cell = row.GetCell(0); p.id = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(1); p.rank = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(2); p.order = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(3); p.name = (cell == null ? "" : cell.StringCellValue);
					cell = row.GetCell(4); p.needPoint = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(5); p.stamina = (int)(cell == null ? 0 : cell.NumericCellValue);
					p.HP = new int[3];
					cell = row.GetCell(6); p.HP[0] = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(7); p.HP[1] = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(8); p.HP[2] = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(9); p.deck = (int)(cell == null ? 0 : cell.NumericCellValue);
					p.ai = new int[3];
					cell = row.GetCell(10); p.ai[0] = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(11); p.ai[1] = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(12); p.ai[2] = (int)(cell == null ? 0 : cell.NumericCellValue);
					p.drop = new int[1];
					cell = row.GetCell(13); p.drop[0] = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(14); p.money = (int)(cell == null ? 0 : cell.NumericCellValue);
						s.list.Add (p);
					}
					data.sheets.Add(s);
				}
			}

			ScriptableObject obj = AssetDatabase.LoadAssetAtPath (exportPath, typeof(ScriptableObject)) as ScriptableObject;
			EditorUtility.SetDirty (obj);
		}
	}
}
