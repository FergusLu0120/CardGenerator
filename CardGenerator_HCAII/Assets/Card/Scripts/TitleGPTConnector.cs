using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenAI;

public class TitleGPTConnector : MonoBehaviour {

	private OpenAIApi openai = new OpenAIApi("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");

	private List<ChatMessage> messages = new List<ChatMessage>();
	private string presetPrompt = "I am deciding a boxing game avatar on a avatar card, the title have to be in two words, please give me one suggestion and reply me in the following format: Title: [XXXX], while XXXX replaced with the title you suggested, DO NOT change my format. I have the following prompts as the element of the title, use some of the prompts as inspiration, no need to include all: ";

	public string TitleResult = "Meta Puncher";

	public async void RequestTitle(string prompt) {
		var newMessage = new ChatMessage() {
			Role = "user",
			Content = string.Format("{0} {1}", presetPrompt, prompt)
		};

		//if (messages.Count == 0) newMessage.Content = prompt + "\n" + inputField.text;
		messages.Clear();
		messages.Add(newMessage);

		// Complete the instruction
		var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest() {
			Model = "gpt-3.5-turbo",
			Messages = messages
		});

		if (completionResponse.Choices != null && completionResponse.Choices.Count > 0) {
			var message = completionResponse.Choices[0].Message;
			message.Content = message.Content.Trim();

			Debug.Log(message.Content);

			string s = message.Content;
			int start = s.IndexOf(":") + 2;
			int end = s.Length;
			//Debug.Log(end);
			//Debug.Log(end - start);
			string result = s.Substring(start, end - start);

			Debug.Log(result);
			TitleResult = result;
			//messages.Add(message);
			//AppendMessage(message);
		} else {
			Debug.LogWarning("No text was generated from this prompt.");
		}

		//key
		//sk - n42ic1V9pgYyIyKPKf06T3BlbkFJOaXxHKisAExTt3owc7yz
	}
}
