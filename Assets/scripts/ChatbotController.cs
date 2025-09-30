using GLTFast.Schema;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ChatbotController : MonoBehaviour
{
    public static ChatbotController Instance;

    [Header("API Settings")]
    [SerializeField] private string geminiApiKey; // Paste your key here in the Inspector

    [Header("UI References")]
    public GameObject chatPanel;
    public Button openChatButton;
    public TMP_InputField userInputField;
    public Button sendButton;
    public Button clearChatButton;
    public TextMeshProUGUI chatHistoryText; // The text inside your Scroll View
    public ScrollRect chatScrollRect;
    private const string GeminiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-latest:generateContent?key=";
    private List<Content> conversationHistory = new List<Content>();
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // This correctly protects the entire Canvas from being destroyed on scene load.
            DontDestroyOnLoad(this.transform.root.gameObject);
        }
        else
        {
            // This is the CRITICAL FIX: It destroys the entire duplicate canvas, not just this script's GameObject.
            Destroy(this.transform.root.gameObject);
            return;
        }

        // --- The rest of your Awake logic ---

        // It is best to wire up the Toggle button in the Inspector, so we will remove the line that was here.
        sendButton.onClick.AddListener(OnSendButtonClicked);
        if (clearChatButton != null)
        {
            clearChatButton.onClick.AddListener(ClearChat);
        }
    }
    public void ClearChat()
    {
        chatHistoryText.text = "Gyani: Namaste! How can I help you explore India's heritage today?";
        conversationHistory.Clear();
        Debug.Log("Chat history cleared.");
    }
    public void ShowChatPanel() => chatPanel.SetActive(true);
    public void HideChatPanel() => chatPanel.SetActive(false);

    private void OnSendButtonClicked()
    {
        string userInput = userInputField.text;
        if (string.IsNullOrWhiteSpace(userInput)) return;

        AppendMessageToHistory("You: " + userInput);
        userInputField.text = "";

        // --- NEW: Add the user's message to the history list ---
        conversationHistory.Add(new Content
        {
            role = "user",
            parts = new[] { new Part { text = userInput } }
        });

        StartCoroutine(SendRequestToGemini()); // No longer needs to pass the input string
    }
    public void ToggleChatPanel() => chatPanel.SetActive(!chatPanel.activeSelf);
    private IEnumerator SendRequestToGemini()
    {
        string url = GeminiEndpoint + geminiApiKey;
        sendButton.interactable = false;

        // --- Create the JSON Payload (Corrected Structure) ---

        // The system prompt defines the AI's personality and rules.
        string systemPrompt = @"
Persona: You are 'Gyani', a friendly and engaging virtual tour guide for the BharatLok app.

Core Task (Strict): Answer only questions about Indian heritage, monuments, or historical data related to them. 
If the question is unrelated, respond exactly with: 'I can only answer questions about India's history, monuments, and culture.'

Answer Rules:
- The main answer must be in exactly one sentence.
- Then add one short, intriguing follow-up fact starting with 'Did you know...!'—also in exactly one sentence.

Do NOT answer anything outside this scope.
";


        // Create a temporary list of content to send.
        List<Content> contentToSend = new List<Content>();

        // If this is the VERY FIRST message of a new conversation...
        if (conversationHistory.Count == 1)
        {
            // ...combine the system prompt with the user's first question.
            string firstMessage = systemPrompt + "\n\nUSER QUESTION: " + conversationHistory[0].parts[0].text;
            contentToSend.Add(new Content
            {
                role = "user",
                parts = new[] { new Part { text = firstMessage } }
            });
        }
        else
        {
            // For all subsequent messages, just send the existing conversation history.
            contentToSend = conversationHistory;
        }

        var requestData = new GeminiRequest
        {
            contents = contentToSend.ToArray(),
            safetySettings = new[]
            {
            new SafetySetting { category = "HARM_CATEGORY_HARASSMENT", threshold = "BLOCK_NONE" },
            new SafetySetting { category = "HARM_CATEGORY_HATE_SPEECH", threshold = "BLOCK_NONE" },
            new SafetySetting { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "BLOCK_NONE" },
            new SafetySetting { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "BLOCK_NONE" }
        }
        };

        string jsonPayload = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);

        // --- Send the Web Request ---
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Gemini Error: {request.error} - {request.downloadHandler.text}");
                AppendMessageToHistory("Gyani: Sorry, an error occurred. Please try again.");
                // If an error occurs, remove the last user message from history to allow a retry
                if (conversationHistory.Count > 0)
                {
                    conversationHistory.RemoveAt(conversationHistory.Count - 1);
                }
            }
            else
            {
                var response = JsonUtility.FromJson<GeminiResponse>(request.downloadHandler.text);
                string aiResponse = response.candidates[0].content.parts[0].text;
                AppendMessageToHistory("Gyani: " + aiResponse);

                // Add the AI's response to the history list for the next turn
                conversationHistory.Add(new Content
                {
                    role = "model",
                    parts = new[] { new Part { text = aiResponse } }
                });
            }
        }
        sendButton.interactable = true;
    }
    private void AppendMessageToHistory(string message)
    {
        // Add a newline only if there's existing text
        if (!string.IsNullOrEmpty(chatHistoryText.text))
        {
            chatHistoryText.text += "\n\n";
        }
        chatHistoryText.text += message;
        Canvas.ForceUpdateCanvases();
        chatScrollRect.verticalNormalizedPosition = 0f;
    }
}
// --- Helper classes for JSON Serialization (Corrected Version) ---

// The main request object, now with system instructions
[System.Serializable]
public class GeminiRequest
{
    //public Content system_instruction;
    public Content[] contents;
    public GenerationConfig generationConfig;
    public SafetySetting[] safetySettings;
}

[System.Serializable]
public class Content
{
    public string role;
    public Part[] parts;
}

[System.Serializable]
public class Part
{
    public string text;
}

// Optional settings for the model's generation
[System.Serializable]
public class GenerationConfig
{
    public int candidateCount = 1;
    public float temperature = 0.7f; // Controls creativity
}

// Prevents the AI from being overly cautious
[System.Serializable]
public class SafetySetting
{
    public string category;
    public string threshold;
}

// Classes for parsing the response
[System.Serializable]
public class GeminiResponse { public Candidate[] candidates; }

[System.Serializable]
public class Candidate { public Content content; }