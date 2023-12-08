using LibSasara;
using WanaKanaNet;

//コマンドライン引数の一つ目はlabファイルへのパス
var labPath = args[0];

//Labファイル読み込み
var lab = await SasaraLabel.LoadAsync(labPath);

//音素からカナ変換の特別ルール
WanaKanaOptions kanaOption = new()
{
	CustomKanaMapping = new Dictionary<string, string>()
	{
		{"cl","っ"},
		{"di","でぃ"}
	}
};

var output = lab
	//小節・フレーズ単位で分割（0.1秒空白毎）
	.SplitToSentence(0.1)
	//必要な情報だけ抜き出してリスト化
	.Select(v => (
		Start: v[0].From / 10000000,    //開始
		End: v[^1].To / 10000000,   //終了
		Phrase: string.Join("", v.Select(s => s.Phoneme))   //音素文字列
	))
	//音素 -> ひらがな変換
	.Select(v => (
		Start: v.Start,
		End: v.End,
		Phrase: WanaKana.ToHiragana(v.Phrase, kanaOption)
	))
	//字幕(.srt)フォーマットに変換
	.Select((v,i)=> $"""
{i+1}
{TimeSpan.FromSeconds(v.Start).ToString(@"hh\:mm\:ss\,fff")} --> {TimeSpan.FromSeconds(v.End).ToString(@"hh\:mm\:ss\,fff")}
{v.Phrase}

"""
	)
	;

//labファイルと同じ名前で拡張子を.srtに変換して保存
await File.WriteAllLinesAsync(
	Path.ChangeExtension(labPath, "srt"),
	output
);