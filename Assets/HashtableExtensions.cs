using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Hashtable 型の拡張メソッドを管理するクラス
/// </summary>
public static partial class HashtableExtensions
{
	/// <summary>
	/// 指定された Hashtable を Dictionary<string, string> に変換します
	/// </summary>
	/// <param name="self">Hashtable 型のインスタンス</param>
	/// <returns>Dictionary<string, string> 型のインスタンス</returns>
	public static Dictionary<string, string> ToDictionary( this Hashtable self )
	{
		var result = new Dictionary<string, string>();
		foreach ( DictionaryEntry n in self )
		{
			result[ n.Key.ToString() ] = n.Value.ToString();
		}
		return result;
	}
}