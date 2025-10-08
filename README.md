# 🇮🇳 BharatLok AR

BharatLok AR is a mobile application that acts as a digital time machine, using Augmented Reality to transform the experience of visiting Indian cultural heritage sites. It replaces static information plaques with immersive, interactive 3D models and a personal AI tour guide, making history engaging and accessible for everyone.

This project was selected as a college winner from over 600+ competing teams to represent Galgotias College of Engineering and Technology in the Smart India Hackathon (SIH) 2025.

---

## The Problem
Many of India's magnificent historical sites are experienced passively. Visitors, especially younger audiences, often struggle to connect with dense text on information plaques and find it difficult to visualize how ruined structures looked in their prime. This creates a disconnect from the rich history and architectural genius behind these monuments.

## The Solution
BharatLok AR bridges this gap by leveraging the power of Augmented Reality. By simply pointing their smartphone at a monument, users can view a photorealistic 3D reconstruction overlaid onto the real world. They can explore the structure from all angles, interact with different parts, and even ask questions to an AI guide, turning a passive visit into an active, memorable learning journey.

---

## 📷 Live Demo

[INSERT A HIGH-QUALITY GIF OR YOUTUBE VIDEO OF YOUR APP IN ACTION HERE]
*A visual demonstration is critical. Show a user pointing the phone at a location, the 3D model appearing, and them tapping on it to get information.*

---

## ✨ Key Features Explained

- **Immersive 3D Reconstructions:** Renders high-fidelity, historically accurate 3D models in AR, allowing users to see magnificent structures as they once stood and explore intricate details that are no longer visible today.
- **Interactive Hotspots:** Users can tap on specific parts of the 3D models (e.g., a carving, a collapsed section) to trigger animations, view archival images, and read or listen to detailed historical context.
- **Conversational AI Guide:** Powered by the **Google Gemini API**, the app features a friendly AI guide that can answer users' natural language questions ("Why was this built?", "What is this carving?"), making learning feel like a personal conversation with a historian.
- **Shared AR Experiences:** Using **Google Cloud Anchors**, a family or a group of friends can see and interact with the same AR content from their own devices simultaneously, creating a collaborative and shared educational experience at the site.

---

## ⚙️ How It Works

1.  **Site Recognition:** The app uses the device's GPS to identify the user's location at a registered heritage site.
2.  **AR World Tracking:** Upon launching the AR view, **AR Foundation** initiates its SLAM algorithm to detect surfaces (like the ground) and begin tracking the device's position in the real world.
3.  **Model Placement:** The corresponding 3D model for the monument is loaded and placed at a pre-defined location. If a Cloud Anchor exists, it's used to ensure a precise, persistent placement for all users.
4.  **User Interaction:** C# scripts handle user input, such as taps on the model's interactive hotspots, triggering UI and animation events.
5.  **AI Conversation:** User questions are sent as prompts to the Gemini API via a RESTful call. The returned response is parsed and displayed in a conversational UI, providing dynamic information.

---

## 🛠️ Tech Stack Deep Dive

- **Engine & Language:** **Unity 3D** and **C#**, chosen for its powerful real-time 3D rendering capabilities and cross-platform support for mobile devices.
- **Augmented Reality:** **AR Foundation**, used as a high-level API to access native ARCore (Android) and ARKit (iOS) features for world tracking, plane detection, and rendering.
- **AI & ML:** **Google Gemini API**, integrated to power the intelligent, conversational tour guide feature.
- **Cloud & Persistence:** **Google Cloud Anchors** to enable the persistent, multi-user shared AR experiences.
- **Backend & Data:** **Firebase** for storing monument data, user analytics, and hosting content that the application fetches in real-time.

---

## 🧠 Challenges & Future Work

- **Current Challenge:** Optimizing high-polygon 3D models for smooth, real-time performance across a wide range of mobile devices without sacrificing visual quality.
- **Current Challenge:** Fine-tuning the prompt engineering for the Gemini API to ensure the AI guide provides factually accurate, context-aware, and engaging responses.

- **Future Work:**
    - Adding multilingual support for both UI text and AI-powered narration.
    - Developing a "Then & Now" feature with a slider to compare the 3D historical reconstruction with the live camera view of the current ruins.
    - Integrating gamification elements like historical scavenger hunts and quizzes to further boost user engagement.

---

## 👥 Contributors

- **Ayush Shukla** - *Team Lead & Unity Developer* - [GitHub](https://github.com/technospes)

---

## 🤝 Contributing

Contributions are what make the open-source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement". Don't forget to give the project a star! Thanks again!

1.  **Fork** the Project
2.  Create your **Feature Branch** (`git checkout -b feature/AmazingFeature`)
3.  **Commit** your Changes (`git commit -m 'Add some AmazingFeature'`)
4.  **Push** to the Branch (`git push origin feature/AmazingFeature`)
5.  Open a **Pull Request**

### Reporting Bugs
If you encounter a bug, please open an issue and provide detailed steps to reproduce the problem.

---
