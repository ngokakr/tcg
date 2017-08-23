using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;
using System.Xml.Serialization;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

public class CardPack_importer : AssetPostprocessor {
	private static readonly string filePath = "Assets/ExcelData/CardPack.xls";
	private static readonly string exportPath = "Assets/ExcelData/CardPack.asset";
	private static readonly string[] sheetNames = { "第一弾 -BASIC-", };
	
	static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		foreach (string asset in importedAssets) {
			if (!filePath.Equals (asset))
				continue;
				
			XLS_CardPack data = (XLS_CardPack)AssetDatabase.LoadAssetAtPath (exportPath, typeof(XLS_CardPack));
			if (data == null) {
				data = ScriptableObject.CreateInstance<XLS_CardPack> ();
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

					XLS_CardPack.Sheet s = new XLS_CardPack.Sheet ();
					s.name = sheetName;
				
					for (int i=1; i<= sheet.LastRowNum; i++) {
						IRow row = sheet.GetRow (i);
						ICell cell = null;
						
						XLS_CardPack.Param p = new XLS_CardPack.Param ();
						
					cell = row.GetCell(0); p.id = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(1); p.cardAtr = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(2); p.cardID = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(3); p.cardLV = (int)(cell == null ? 0 : cell.NumericCellValue);
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
