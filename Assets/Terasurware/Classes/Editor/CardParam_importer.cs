using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;
using System.Xml.Serialization;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

public class CardParam_importer : AssetPostprocessor {
	private static readonly string filePath = "Assets/ExcelData/CardParam.xls";
	private static readonly string exportPath = "Assets/ExcelData/CardParam.asset";
	private static readonly string[] sheetNames = { "Sheet0","Sheet1","Sheet2","Sheet3","Sheet4", };
	
	static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		foreach (string asset in importedAssets) {
			if (!filePath.Equals (asset))
				continue;
				
			XLS_CardParam data = (XLS_CardParam)AssetDatabase.LoadAssetAtPath (exportPath, typeof(XLS_CardParam));
			if (data == null) {
				data = ScriptableObject.CreateInstance<XLS_CardParam> ();
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

					XLS_CardParam.Sheet s = new XLS_CardParam.Sheet ();
					s.name = sheetName;
				
					for (int i=1; i<= sheet.LastRowNum; i++) {
						IRow row = sheet.GetRow (i);
						ICell cell = null;
						
						XLS_CardParam.Param p = new XLS_CardParam.Param ();
						
					cell = row.GetCell(0); p.id = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(1); p.attribute = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(2); p.role = (cell == null ? "" : cell.StringCellValue);
					cell = row.GetCell(3); p.reality = (int)(cell == null ? 0 : cell.NumericCellValue);
					p.group = new int[3];
					cell = row.GetCell(4); p.group[0] = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(5); p.group[1] = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(6); p.group[2] = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(7); p.name = (cell == null ? "" : cell.StringCellValue);
					cell = row.GetCell(8); p.cost = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(10); p.power = (int)(cell == null ? 0 : cell.NumericCellValue);
					p.skill = new string[3];
					cell = row.GetCell(12); p.skill[0] = (cell == null ? "" : cell.StringCellValue);
					cell = row.GetCell(13); p.skill[1] = (cell == null ? "" : cell.StringCellValue);
					cell = row.GetCell(14); p.skill[2] = (cell == null ? "" : cell.StringCellValue);
					p.script = new string[3];
					cell = row.GetCell(15); p.script[0] = (cell == null ? "" : cell.StringCellValue);
					cell = row.GetCell(16); p.script[1] = (cell == null ? "" : cell.StringCellValue);
					cell = row.GetCell(17); p.script[2] = (cell == null ? "" : cell.StringCellValue);
					p.effect = new int[3];
					cell = row.GetCell(18); p.effect[0] = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(19); p.effect[1] = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(20); p.effect[2] = (int)(cell == null ? 0 : cell.NumericCellValue);
					p.value = new int[3];
					cell = row.GetCell(21); p.value[0] = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(22); p.value[1] = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(23); p.value[2] = (int)(cell == null ? 0 : cell.NumericCellValue);
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
