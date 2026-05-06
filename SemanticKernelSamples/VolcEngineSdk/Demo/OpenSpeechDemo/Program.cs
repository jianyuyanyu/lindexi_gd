// See https://aka.ms/new-console-template for more information

var accessTokenFile = @"C:\lindexi\Work\Key\OpenSpeech TTS Access Token.txt";
var secretKeyFile = @"C:\lindexi\Work\Key\OpenSpeech TTS Secret Key.txt";

// speaker 发音人： voiceType
var voiceType = "zh_female_vv_uranus_bigtts";

// 将 use_tag_parser 开启。开启cot解析能力。cot能力可以辅助当前语音合成，对语速、情感等进行调整
// 传递内容为：
// <cot text=急促难耐>工作占据了生活的绝大部分</cot>，只有去做自己认为伟大的工作，才能获得满足感。<cot text=语速缓慢>不管生活再苦再累，都绝不放弃寻找</cot>

// 模型版本： seed-tts-2.0-expressive
Console.WriteLine("Hello, World!");
