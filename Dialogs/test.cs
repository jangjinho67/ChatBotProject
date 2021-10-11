using System;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Net.Http;
using System.Xml;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Project
{
    [Serializable]
    public class test : IDialog<object>
    {
        // 사용자, 컴퓨터 리스트
        List<string> user = new List<string>();
        List<string> computer = new List<string>();
        // 두음법칙 적용을 위한 딕셔너리
        Dictionary<char, char> converts = new Dictionary<char, char>()
        {
            {'라', '나'},
            {'락', '낙'},
            {'란', '난'},
            {'랄', '날'},
            {'람', '남'},
            {'랍', '납'},
            {'랏', '낫'},
            {'랑', '낭'},
            {'략', '약'},
            {'량', '양'},
            {'렁', '넝'},
            {'려', '여'},
            {'녀', '여'},
            {'력', '역'},
            {'녁', '역'},
            {'련', '연'},
            {'년', '연'},
            {'렬', '열'},
            {'렴', '염'},
            {'념', '염'},
            {'렵', '엽'},
            {'령', '영'},
            {'녕', '영'},
            {'로', '노'},
            {'록', '녹'},
            {'론', '논'},
            {'롤', '놀'},
            {'롬', '놈'},
            {'롭', '놉'},
            {'롯', '놋'},
            {'롱', '농'},
            {'료', '요'},
            {'뇨', '요'},
            {'룡', '용'},
            {'뇽', '용'},
            {'루', '누'},
            {'룩', '눅'},
            {'룬', '눈'},
            {'룰', '눌'},
            {'룸', '눔'},
            {'룻', '눗'},
            {'룽', '눙'},
            {'류', '유'},
            {'뉴', '유'},
            {'륙', '육'},
            {'률', '율'},
            {'르', '느'},
            {'륵', '늑'},
            {'른', '는'},
            {'를', '늘'},
            {'름', '늠'},
            {'릅', '늡'},
            {'릇', '늣'},
            {'릉', '능'},
            {'래', '내'},
            {'랙', '낵'},
            {'랜', '낸'},
            {'랠', '낼'},
            {'램', '냄'},
            {'랩', '냅'},
            {'랫', '냇'},
            {'랭', '냉'},
            {'례', '예'},
            {'뢰', '뇌'},
            {'리', '이'},
            {'니', '이'},
            {'린', '인'},
            {'닌', '인'},
            {'릴', '일'},
            {'닐', '일'},
            {'림', '임'},
            {'님', '임'},
            {'립', '입'},
            {'닙', '입'},
            {'릿', '잇'},
            {'닛', '잇'},
            {'링', '잉'},
            {'닝', '잉'},
        };
        // 국립국어원 API
        string link = "https://stdict.korean.go.kr/api/search.do?certkey_no=2356&key=";
        string key = "9717EC333DE82602C2A671DA6CBFF851";
        string search = "&type_search=search&advanced=y&pos=1&q=";
        string urlString = "";
        // 게임을 진행할 횟수
        int count = 3;
        // 검색된 단어 갯수
        int total = 0;
        // 컴퓨터 리스트에 단어 추가하는 메소드
        bool inputComputer(char data)
        {
            // 사용자로부터 받은 단어 중 끝 단어를 가지고 사전 검색
            if (wordYes(data).Item1)
            {
                while (true)
                {
                    string temp = wordYes(data).Item2;
                    // 두 개의 리스트에 없는 값인 경우 단어 추가
                    if (!computer.Contains(temp) && !user.Contains(temp))
                    {
                        computer.Add(temp);
                        // 다음 절차를 위한 true 값 반환
                        return true;
                    }
                }
            }
            // 단어가 없는 경우
            else
                return false;
        }
        // 사용자로부터 입력받은 단어 검색하는 메소드
        bool wordYes(string data)
        {
            // 단어를 토대로 사전 재검색
            urlString = link + key + search + data;
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = false;
            doc.Load(urlString);
            // 검색된 총 단어 갯수 불러오기
            XmlNode ltotal = doc.SelectNodes("//channel/total")[0];
            total = Int32.Parse(ltotal.InnerText);
            // 단어가 없는 경우
            if (total == 0)
                return false;
            // 단어가 있는 경우
            else
                return true;
        }
        // 사용자로부터 입력받은 끝 단어를 가지고 컴퓨터 리스트에 저장할 단어와 결과를 반환하는 메소드
        Tuple<bool, string> wordYes(char data)
        {
            // 끝 단어를 가지고 해당 단어로 시작하는 단어를 검색
            Random random = new Random();
            search = "&type_search=search&advanced=y&method=start&pos=1&letter_s=2&q=";
            urlString = link + key + search + data;
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = false;
            doc.Load(urlString);
            // 검색된 총 단어 갯수 불러오기
            XmlNode ltotal = doc.SelectNodes("//channel/total")[0];
            total = Int32.Parse(ltotal.InnerText);
            // 검색된 단어가 없는 경우
            if (total == 0)
            {   
                // 단어가 없지만 두음법칙에 적용되는 단어가 있는 경우
                if (converts.ContainsKey(data))
                {
                    // 단어 변환
                    data = converts[data];
                    // 변환된 끝 단어로 다시 검색
                    return wordYes(data);
                }
                // 단어가 없는 경우 false 반환
                else
                    return new Tuple<bool, string>(false, null);
            }
            // 단어가 존재하는 경우, 총 검색 갯수는 10 (기본값)
            else if (total <= 10)
            {
                // 검색 자료 중 랜덤한 단어 추출
                int specific = random.Next(0, total);
                XmlNode word = doc.SelectNodes("//channel/item/word")[specific];
                // 해당 단어 반환 후 inputComputer 메소드 실행
                return new Tuple<bool, string>(true, word.InnerText);
            }
            // 단어가 존재하는 경우, 검색 갯수가 10개 이상인 경우
            else
            {
                // 총 검색 갯수는 100개로 한정되어 있음
                if (total > 100)
                    total = 100;
                urlString = link + key + "&num=" + total.ToString() + search + data;
                doc.Load(urlString);
                int specific = random.Next(0, total);
                XmlNode word = doc.SelectNodes("//channel/item/word")[specific];
                // 해당 단어 반환 후 inputComputer 메소드 실행
                return new Tuple<bool, string>(true, word.InnerText);
            }
        }
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageChoiceAsync);
        }

        // 히어로 카드 실행
        public async Task MessageChoiceAsync(IDialogContext context,
                                               IAwaitable<object> result)
        {
            var message = context.MakeMessage();
            var actions = new List<CardAction>();

            actions.Add(new CardAction() { Title = "1. 게임 설명", Value = "1", Type = ActionTypes.ImBack });
            actions.Add(new CardAction() { Title = "2. 게임 시작", Value = "2", Type = ActionTypes.ImBack });
            actions.Add(new CardAction() { Title = "3. 종료", Value = "3", Type = ActionTypes.ImBack });

            message.Attachments.Add(
                new HeroCard { Title = "끝말잇기", Buttons = actions }.ToAttachment()
            );

            await context.PostAsync(message);
            context.Wait(SendMessageAsync);
        }

        public async Task SendMessageAsync(IDialogContext context,
                                               IAwaitable<object> result)
        {
            Activity activity = await result as Activity;
            string strSelected = activity.Text.Trim();

            if (strSelected == "1")
            {
                await context.PostAsync("국립 국어원 API 사용\n" +
                    "명사만 사용할 것\n" +
                    "최소 두글자 이상의 단어를 사용할 것\n" +
                    "총 3번의 기회가 주어지며 게임을 이어나갈 시 초기화 됨";
                context.Wait(MessageChoiceAsync);
            }
            else if (strSelected == "2")
            {
                await context.PostAsync("게임을 시작합니다. 먼저 단어를 입력해주세요.");
                context.Wait(MessageReceivedAsync);
            }
            else if (strSelected == "3")
            {
                await context.PostAsync("게임 종료");
                context.Done("게임을 종료합니다.");
            }
            else
            {
                await context.PostAsync("잘못 입력하셨습니다.");
                context.Wait(MessageChoiceAsync);
            }
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            // 사용자로 부터 입력받은 단어 값 저장
            var input = await argument;
            string data = input.Text;
            // 처음 실행할 때, 단어의 유무만 빠르게 판별
            if (computer.Count == 0)
            {
                // 단어가 없는 경우
                if (!wordYes(data))
                {
                    count = count - 1;
                    await context.PostAsync($"존재하지 않는 단어입니다, {count}번의 기회가 남았습니다.");
                }
                // 단어가 존재하는 경우
                else
                {
                    // temp 변수는 사용자가 입력한 단어의 마지막 값
                    char temp = data[data.Length - 1];
                    count = 3;
                    // 사용자 리스트에 추가
                    user.Add(data);
                    // 마지막 단어를 인수로 갖는 컴퓨터 리스트에 추가하기 위한 메소드 실행
                    if (inputComputer(temp))
                        // 입력된 단어 출력
                        await context.PostAsync($"{computer[computer.Count - 1]}");
                    else
                    {
                        await context.PostAsync($"{temp}로 시작하는 단어가 없습니다. 당신이 이겼어요");
                    }
                }
            }
            // 처음 실행이 아닌 두번째 이상의 실행 경우
            else
            {
                // temp는 사용자가 입력한 첫 글자, comptemp는 컴퓨터 리스트에 있는 가장 마지막 단어의 마지막 글자
                char temp = data[0];
                char comtemp = computer[computer.Count - 1][computer[computer.Count - 1].Length - 1];
                // 두 단어를 비교해서 다른 경우
                if (temp != comtemp)
                {
                    count = count - 1;
                    await context.PostAsync($"마지막 글자가 서로 일치하지 않습니다. {count}번의 기회가 남았습니다.");
                }
                // 단어가 일치하는 경우
                else
                {
                    // 단어가 존재하지 않는 경우
                    if (!wordYes(data))
                    {
                        count = count - 1;
                        await context.PostAsync($"존재하지 않는 단어입니다, {count}번의 기회가 남았습니다.");
                    }
                    else
                    {   
                        // 이미 사용된 단어인 경우
                        if (user.Contains(data))
                        {
                            count = count - 1;
                            await context.PostAsync($"이미 사용된 단어입니다. {count}번의 기회가 남았습니다.");
                        }
                        // 이미 사용된 단어인 경우
                        else if (computer.Contains(data))
                        {
                            count = count - 1;
                            await context.PostAsync($"이미 사용된 단어입니다. {count}번의 기회가 남았습니다.");
                        }
                        // 처음으로 입력된 단어인 경우
                        else
                        {
                            // temp 변수는 사용자가 입력한 단어의 마지막 값
                            temp = data[data.Length - 1];
                            count = 3;
                            // 사용자 리스트에 추가
                            user.Add(data);
                            // 마지막 단어를 인수로 갖는 컴퓨터 리스트에 추가하기 위한 메소드 실행
                            if (inputComputer(temp))
                                // 입력된 단어 출력
                                await context.PostAsync($"{computer[computer.Count - 1]}");
                            else
                            {
                                await context.PostAsync($"{temp}로 시작하는 단어가 없습니다. 당신이 이겼어요");
                            }
                        }
                    }
                }
            }
            // 기회가 모두 소진된 경우 게임 종료
            if (count == 0)
            {
                await context.PostAsync("남은 기회가 없으므로 게임이 종료되었습니다");
                context.Done("게임을 다시 하려면 아무 채팅이나 입력해주세요");
            }
            context.Wait(MessageReceivedAsync);
        }
    }
}