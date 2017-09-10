using System.Collections;
using System.Collections.Generic;
//using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.EventSystems;



public interface IRecieveMessage :  IEventSystemHandler {
	void OnRecieve(int _num,int _tag);
}

public interface ICardDragHandler : IEventSystemHandler {
	
	void OnCardTap (int _num, int _tag);

	void OnVartical (int _value,int _num, int _tag);
	void OnHorizontal (int _value, int _num, int _tag);
}

public class SystemScript : MonoBehaviour {

	public enum ShowType {
		SMART,
		DETAIL,
		BATTLE,
	}

	[System.Serializable]
	public struct CardData {
		public int Atr;
		public int ID;
		public int LV;
		public int Count;
		public int uid;//識別id(バトル専用)
		public CardData Set (int _Atr,int _ID,int _LV,int _Count,int _Unique_id = -1) {
			Atr = _Atr;
			ID = _ID;
			LV = _LV;
			Count = _Count;
			uid = _Unique_id;
			return this;
		}
		public CardData Set (CardParam _cp){
			Atr = _cp.Atr;
			ID = _cp.ID;
			LV = _cp.LV;
			Count = _cp.Count;
			uid = _cp.uid;
			return this;
		}
		public void ChangeCount (int _Delta = 1) {
			Count += _Delta;
		}

		public List<int> ToList () {
			return new List<int> (){ ID, LV, Count };
		}
		public CardData FromList (List<int> data) {
			Atr = 0;
			ID = data [0];
			LV = data [1];
			Count = data [2];
			uid = -1;
			return this;
		}
	}
	[System.Serializable]
	public struct CardParam {
		public int Atr;
		public int ID;
		public int LV;
		public int uid;//識別id(バトル専用)
		public int Count;


		public int Role;
		public int Rea;
		public List<int> Groups;
		public string Name;
		public int Cost;
		public int Power;
		public List<string> SkillTexts;
		public List<string> SkillScript;
		public List<int> Effect;
		public List<int> EffectValue;//持続ターンや消費ポイントなど
		public int Pass;

		public int GetCalCost () {
			int tempCost = 0;
			for (int i = 0; i < Effect.Count; i++ ){
				int effNum = Effect [i];
				if (effNum == 12) {//コスト補正
					tempCost = EffectValue[i];
				}
			}
			return Cost + tempCost;
		}

		public int GetCalPower () {
			int tempPower = 0;
			for (int i = 0; i < Effect.Count; i++ ){
				int effNum = Effect [i];
				if (effNum == 13) {//パワー補正
					tempPower = EffectValue[i];
				}
			}
			return Power + tempPower;
		}

		public CardParam Reset () {
			ID = -1;
			Atr = -1;
			LV = -1;
			Count = -1;
			Role = -1;
			Rea = -1;
			Groups = new List<int> ();
			Name = "";
			Cost = 0;
			Power = 0;
			SkillTexts = new List<string> ();
			SkillScript = new List<string> ();
			Effect = new List<int> ();
			EffectValue = new List<int> ();
			Pass = -1;
			return this;
		}
		public CardParam Set (int _atr,int _id,int _lv,int _count) {
			CardData cd = new CardData ().Set (_atr, _id, _lv, _count);
			return Set (cd);
		}
		public CardParam Set (CardData _cd) {
			Atr = _cd.Atr;
			ID = _cd.ID;
			LV = _cd.LV;
			Count = _cd.Count;
			uid = _cd.uid;
				
			XLS_CardParam.Param param = DataManager.Instance.GetCardParam (_cd);
			Role = GetRoleInt( param.role);
			Rea = param.reality;
			Groups = new List<int> ();
			for (int i = 0; i < param.group.Length; i++ ){
				int temp = param.group [i];
				if (temp != 0)
					Groups.Add (temp);
			}
			Name = param.name;
			Cost = param.cost;
//			Power = AdjustedPower(Rea,param.power,LV);
			Power = param.power;
			SkillTexts = new List<string> ();
			SkillScript = new List<string> ();
			Effect = new List<int> ();
			EffectValue = new List<int> ();

			for (int i = 0; i < param.skill.Length; i++ ){
				if (param.skill [i] != "") {
					SkillTexts.Add (param.skill [i]);
					SkillScript.Add (param.script [i]);
				}
			}
			for (int i = 0; i < param.effect.Length; i++ ){
				int eff = param.effect [i];
				int effValue = param.value[i];
				if (eff > 0) {
					Effect.Add (eff);
					EffectValue.Add (effValue);
				}
			}
			return this;
		}
	}

	/// <summary>
	/// 同名のカードをまとめる。
	/// </summary>
	public static List<CardParam> PackCardParams (List<CardParam> idSortedData){
		List<CardParam> ret = new List<CardParam> ();
		CardParam temp = new CardParam ().Reset ();
		for (int i = 0; i < idSortedData.Count; i++ ){
			CardParam cp = idSortedData [i];
			if (cp.Atr == temp.Atr && cp.ID == temp.ID) {
				temp.Count += cp.Count;
			} else {
				//現在のデータで確定
				if (temp.Atr == -1 || temp.ID == -1) {
					//-1,-1は無視する。
				} else {
					ret.Add (temp);
				}
				temp = cp;
			}
		}
		return ret;
	}


	//カードシャッフル
	public static List<CardParam> ShuffleCP (List<CardParam> _lcp) {
		for (int i = 0; i < _lcp.Count; i++ ){
			CardParam temp = _lcp [i];
			int randomIndex = Random.Range (0, _lcp.Count);
			_lcp [i] = _lcp [randomIndex];
			_lcp [randomIndex] = temp;
		}
		return _lcp;
	}

	//パワー
	public static int AdjustedPower (int _reality,int _maxPower,int _lv) {
		int maxLv = DataManager.Instance.MaxLV [_reality];
		return _maxPower - maxLv + _lv;
	}


	public static List<CardParam> cdTocp (List<CardData> _lcd) {
		List<CardParam> lcp = new List<CardParam> ();
		for (int i = 0; i < _lcd.Count; i++ ){
			lcp.Add(new CardParam().Set(_lcd[i]));
		}
		return lcp;
	}
	//Dictionaryからカードを取り出すキー
	public static string GetKey(int _atr,int _ID) {
		return _atr + "_" + _ID;
	}

	// Use this for initialization
	void Start () {
		
	}

	public static List<CardParam> GetEnemyDeck (int _deckNum) {
		List<CardParam> ret = new List<CardParam> ();
		List<XLS_EnemyDeck.Param> paramList = DataManager.Instance.xls_EnemyDeck.sheets [_deckNum].list;
		for (int i = 0; i < paramList.Count; i++ ){
			XLS_EnemyDeck.Param param = paramList [i];
			CardParam cp = new CardParam ().Set (param.cardAtr, param.cardID, param.cardLV,param.cardCount);
			ret.Add (cp);
		}
		return ret;
	}
	public static Sprite GetCardSprite (CardData _cd){
		string ResoursePass = DataManager.Instance.fileNames.CardResourseFolder;
		return Resources.Load(ResoursePass + "/"+_cd.Atr+"/"+DataManager.Instance.fileNames.CardName+_cd.ID,typeof(Sprite)) as Sprite;
	}

	public static Sprite GetCardSprite (CardParam _cp) {
		return GetCardSprite (new CardData ().Set (_cp));
	}

	public static Sprite GetSprite (string _pass) {
		return Resources.Load(_pass,typeof(Sprite)) as Sprite;
	}
	public static string GetReality (int _rea,bool _Colored = false) {
		string[] rea = {"ノーマル","レア","スーパーレア","ウルトラレア"};
		string[] reaCol = {"ノーマル","<color=#00ff00>レア</color>","<color=#ff00ff>スーパーレア</color>","<color=#ff0000>ウルトラレア</color>"};
		if (_Colored) {
			return reaCol [_rea];
		} else
		return rea [_rea];
	}
	public static string GetGroup (List<int> _Group) {
		string temp = "";
		for (int i = 0; i < _Group.Count; i++ ){
			if (_Group [i] == 0)
				continue;
			if(temp != "")
				temp += " / ";
			temp += DataManager.Instance.xls_Groups.sheets [0].list [_Group [i]].name;
		}
		return temp;
//		DataManager.Instance.xls_Groups.sheets[0].list[
	}


	public static string GetTiming (string _text) {
		return L_GetSquareBrackets (_text);
//
//		//召喚時発動
//		if (_text.Contains ("[s]")) {
//			return "s";
//		}
//		//戦場発動(トリガー)
//		if (_text.Contains ("[f]")) {
//			return "f";
//		}
//		//戦場発動(常在)
//		if (_text.Contains ("[f2]")) {
//			return "f2";
//		}
//		//手札発動(トリガー)
//		if (_text.Contains ("[h]")) {
//			return "h";
//		}
//		//手札発動(常在)
//		if (_text.Contains ("[h2]")) {
//			return "h2";
//		}
//		//割引
//		if (_text.Contains ("[w]")) {
//			return "w";
//		}
//		return "";
	}
	/// <summary>
	/// a=0 c=1 d=2
	/// </summary>
	public static int GetRoleInt (string _role) {
		if (_role == "a")
			return 0;
		if (_role == "c")
			return 1;
		if (_role == "d")
			return 2;
		return -1;
	}
	public static string GetRole (int _role) {
		if (_role == 0)
			return "アタッカー";
		if (_role == 1)
			return "チャージャー";
		if (_role == 2)
			return "ディフェンダー";
		return "不明";
	}
	public static string GetSkillsText (CardParam _cp,ShowType _showType,bool _newLine,int _skillNum = -1) {
		List<string> skillsText = _cp.SkillTexts;
		List<string> skillsScript = _cp.SkillScript;
		List<int> Effects = _cp.Effect;
		List<int> EffectsValue = _cp.EffectValue;
		int lv = _cp.LV;
		string SkillStr = "";

		for (int i = 0; i < Effects.Count; i++ ){
			string text = SystemScript.GetEffectText (Effects[i],EffectsValue[i],_showType,_newLine,i == _skillNum);
			if (text != null) {
				if(SkillStr != "")//かいぎょう
					SkillStr += "\n";
				SkillStr += text;//追加
			}
		}

		for (int i = 0; i < skillsText.Count; i++ ){
			string text = SystemScript.GetSkillText (skillsText[i],lv,_showType,_newLine,i == _skillNum);
			if (text != null) {
				if(SkillStr != "")//かいぎょう
					SkillStr += "\n";
				SkillStr += text;//追加
			}
		}
		return SkillStr;
	}

	public static string GetEffectText (int _Effect,int _Value,ShowType _showType,bool _newLine,bool _useSkill) {
		if (_Effect == 0)
			return null;
		string text = "";

		var param = DataManager.Instance.xls_ParamEffect.sheets [0].list [_Effect];

		//名前
		if (param.name.Contains ("{0}")) {
			text += "■";
			text += string.Format (param.name, _Value);
		} else {
			text += "■";
			text += param.name;
		}

		//詳細
		if (_showType == ShowType.BATTLE || _showType == ShowType.DETAIL) {
			if (param.description.Contains ("{0}")) {
				text += " (";
				text += string.Format (param.description,_Value);
				text += ")";
			} else {
				text += " (";
				text += param.description;
				text += ")";
			}	

		}
		return text;
	}
	public static string GetSkillText (string _skillText,int _lv,ShowType _showType,bool _newLine,bool _useSkill) {
		if (_skillText == null || _skillText == "")
			return null;

		string text = "";
		if (_skillText.Contains ("[lv")) {
			int vIndex = _skillText.IndexOf ('v');
			vIndex++;
			int needLv = int.Parse (_skillText.Substring (vIndex, 1));
			Debug.Log (needLv + "必要" + _lv);
			if (_lv < needLv) {
				text += "<color=#888888>■LV." + needLv + "で解放</color>";
				if (_showType == ShowType.DETAIL) {
					return text;
				} else {
					return null;
				}
			}
		} else {
		}
//		if (_skillText.Contains ("[e]")) {
//			text += "■";
//			string temp = _skillText.Replace ("[e]", "");
//			XLS_ParamEffect.Param param = DataManager.Instance.xls_ParamEffect.sheets [0].list [int.Parse(temp)];
//			text += param.name;
//
//			//詳細
//			if (_showType == ShowType.BATTLE || _showType == ShowType.DETAIL) {
//				text += " (";
//				text += param.description;
//				text += ")";
//			}
//			_skillText = "";
//		}
		if (_skillText.Contains ("[s]")) {
			text += "<color=#00ffff>■召喚時発動■</color>";
			_skillText = _skillText.Replace ("[s]", "");
			if (_newLine)
				text += "\n";
		}
		if (_skillText.Contains ("[f]")) {
			text += "<color=#00ff00>■戦場発動■</color>";
			_skillText = _skillText.Replace ("[f]", "");
			if (_newLine)
				text += "\n";
		}
		if (_skillText.Contains ("[h]")) {
			text += "<color=#ffff00>■手札発動■</color>";
			_skillText = _skillText.Replace ("[h]", "");
			if (_newLine)
				text += "\n";
		}
		if (_skillText.Contains ("[w]")) {
			text += "<color=#ff00ff>■割引■</color>";
			_skillText = _skillText.Replace ("[w]", "");
			if (_newLine)
				text += "\n";
		}
		if (_skillText.Contains ("[a]")) {
			text += "<color=#ff66ff>■攻撃成功時■</color>";
			_skillText = _skillText.Replace ("[a]", "");
			if (_newLine)
				text += "\n";
		}
		if (_skillText.Contains ("[d]")) {
			text += "<color=#ff6600>■防御成功時■</color>";
			_skillText = _skillText.Replace ("[d]", "");
			if (_newLine)
				text += "\n";
		}
		if (_skillText.Contains ("[jt")) {//ジャストターン
			if (_skillText.Contains ("[ot"))
				Debug.LogError ("jtとotは併用不可");
			//ターン
			int point = int.Parse (GetRoundBrackets(_skillText));
			//テキスト追加
			text += "ジャストターン "+point + " : ";
			//[jt(10)]などのキーワードを削除
			_skillText = RemoveKeyword(_skillText,"jt");
		}
		if (_skillText.Contains ("[ot")) {//オーバーターン
			//ターン
			int point = int.Parse (GetRoundBrackets(_skillText));
			//テキスト追加
			text += "オーバーターン "+point + "〜 : ";
			//[ot(10)]などのキーワードを削除
			_skillText = RemoveKeyword(_skillText,"ot");
		}


		if (_useSkill) {
			text += "<color=#00ff00>"+ _skillText+"</color>";
		} else
		text += _skillText;

		//キーワードはマークする。
		if (!_useSkill) {
			var dataParams = DataManager.Instance.xls_ParamEffect.sheets [0].list;
			for (int i = 0; i < dataParams.Count; i++) {
				string n = dataParams [i].name;
				if (n != "")
					text = text.Replace (n, "<color=#ffff00>" + n + "</color>");
			}
		}
		return text;
	}

	/// <summary>
	/// ()内の文字列を取得
	/// </summary>
	public static string GetRoundBrackets (string _text) {
		if (!_text.Contains ("("))
			return "";
		int startIndex = -1;
		int endIndex = -1;

		//現在入れ子になっている数
		int count = 0;

		for (int i = 0; i < _text.Length; i++ ){
			char x = _text [i];
			if (x == '(') {
				count++;
				if(startIndex == -1)
					startIndex = i;
			}
			if (x == ')') {
				count--;
				if(endIndex == -1 && count == 0)
					endIndex = i;
			}
		}
		return _text.Substring (startIndex + 1, endIndex - startIndex - 1);
	}

	/// <summary>
	/// []内の文字列を取得
	/// </summary>
	public static string L_GetSquareBrackets(string _text){
		if (!_text.Contains ("["))
			return "";
		int startIndex = -1;
		int endIndex = -1;
		//		//1つ目の括弧を消す。
		//		int index = _text.IndexOf ("(");
		//		_text.Remove (0, index+1);
		//		count++;
		//現在入れ子になっている数
		int count = 0;
		for (int i = 0; i < _text.Length; i++ ){
			char x = _text [i];
			if (x == '[') {
				count++;
				if(startIndex == -1)
					startIndex = i;
			}
			if (x == ']') {
				count--;
				if(endIndex == -1 && count == 0)
					endIndex = i;
			}
		}

		// "xx(xx(x)x)xx" startIndex = 2 endIndex = 9
		return _text.Substring (startIndex + 1, endIndex - startIndex - 1);
	}
	/// <summary>
	/// 存在しなければnull
	/// </summary>
	public static string RemoveKeyword (string _base,string _keyword){
		if (_base == null || _base == "")
			return null;
		if (_base.Contains ("["+_keyword)) {//ジャストターン
			//削除
			_base = _base.Remove (_base.IndexOf ("["+_keyword), _base.IndexOf ("]") + 1 - _base.IndexOf ("["+_keyword));
			return _base;
		} else 
			return null;
	}

	public static class AES {
		
		//暗号化 http://masa795.hatenablog.jp/entry/2013/04/19/102642 UnityのC#とPHPで暗号化・復号化してみる
		static public string EncryptRJ256(string prm_key, string prm_iv, string prm_text_to_encrypt)
		{
			string sToEncrypt = prm_text_to_encrypt;

			RijndaelManaged myRijndael = new RijndaelManaged();
			myRijndael.Padding = PaddingMode.Zeros;
			myRijndael.Mode = CipherMode.CBC;
			myRijndael.KeySize = 256;
			myRijndael.BlockSize = 256;

			byte[] encrypted;
			byte[] toEncrypt;

			byte[] key = new byte[0];
			byte[] IV = new byte[0];

			key = System.Text.Encoding.UTF8.GetBytes(prm_key);
			IV = System.Text.Encoding.UTF8.GetBytes(prm_iv);

			ICryptoTransform encryptor = myRijndael.CreateEncryptor(key, IV);

			MemoryStream msEncrypt = new MemoryStream();
			CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);

			toEncrypt = System.Text.Encoding.UTF8.GetBytes(sToEncrypt);

			csEncrypt.Write(toEncrypt, 0, toEncrypt.Length);
			csEncrypt.FlushFinalBlock();
			encrypted = msEncrypt.ToArray();

			return (System.Convert.ToBase64String(encrypted));
		}



		//復号化
		static public string DecryptRJ256(string prm_key, string prm_iv, string prm_text_to_decrypt)
		{
			string sEncryptedString = prm_text_to_decrypt;
			RijndaelManaged myRijndael = new RijndaelManaged();

			myRijndael.Padding = PaddingMode.Zeros;
			myRijndael.Mode = CipherMode.CBC;
			myRijndael.KeySize = 256;
			myRijndael.BlockSize = 256;

			byte[] key = new byte[0];
			byte[] IV = new byte[0];

			key = System.Text.Encoding.UTF8.GetBytes(prm_key);
			IV = System.Text.Encoding.UTF8.GetBytes(prm_iv);

			ICryptoTransform decryptor = myRijndael.CreateDecryptor(key, IV);

			byte[] sEncrypted = System.Convert.FromBase64String(sEncryptedString);
			byte[] fromEncrypt = new byte[sEncrypted.Length];

			MemoryStream msDecrypt = new MemoryStream(sEncrypted);
			CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);

			csDecrypt.Read(fromEncrypt, 0, fromEncrypt.Length);

			return (System.Text.Encoding.UTF8.GetString(fromEncrypt));
		}
	}
	// Update is called once per frame
	void Update () {
		
	}

	static public void OpenURL(string url)
	{
		#if UNITY_EDITOR
		Application.OpenURL(url);
		#elif UNITY_WEBGL
		Application.ExternalEval(string.Format("window.open('{0}','_blank')", url));
		#else
		Application.OpenURL(url);
		#endif
		}
}
