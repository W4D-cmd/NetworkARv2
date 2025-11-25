# NetworkARv2 - Networked AR Application

## Project Overview

NetworkARv2 is a networked augmented reality (AR) application developed for the Meme Sharing Project user studies. It enables multiple AR headsets to interact in a shared augmented environment, supporting synchronized AR memes (appearance, location, position, animation) across devices with real-time communication.

The project is built using Unity 6 and targets Meta Quest 3, 3s, and Pro headsets. It uses Unity's Netcode for GameObjects for networking and Meta XR SDK for AR functionality.

### Key Technologies
- Unity 6
- Unity Netcode for GameObjects
- Meta XR SDK/MR Utility Kit
- OpenXR
- Unity Input System
- Universal Render Pipeline (URP)

### Architecture

The application follows a networked architecture with one host device and multiple client devices:
- **Host Device**: PlayerID = 0, functions as the network host (should be kept unworn)
- **Client Devices**: PlayerID = 1-24, each corresponding to unique sets of 3 memes

### Core Components

1. **MemeNetworkManager**: Central networking component managing meme spawning, synchronization, and state persistence
2. **MemeObjectHandler**: Handles individual meme object state, placement, and network synchronization
3. **PlayerIdManager**: Manages player identification and user interface for ID assignment
4. **MemeMenu**: Provides UI for meme selection and interaction
5. **Logger**: Network-aware logging system that aggregates logs from all connected clients

## Building and Running

### Prerequisites
- Unity 6
- Meta Quest 3, Quest 3s, or Meta Quest Pro
- All devices must be connected to the same Wi-Fi network

### Build Process
1. Open the Unity project
2. Build the sample scene located at: `Assets/Scenes/SampleScene`
3. Deploy the build to each headset

### Running the Application
1. **Host Setup**: Launch the application on the host device first
   - When prompted, set PlayerID = 0
   - Click Enter to join the network
   - Keep the host device unworn (functions as network host)

2. **Client Setup**: After ~5 seconds, start the application on client devices
   - When prompted, set PlayerID = 1-24
   - Each PlayerID corresponds to a unique set of 3 memes
   - Click Enter to join the network

### Network Architecture
The application requires at least three AR headsets:
- One network host (PlayerID 0)
- Multiple client devices (PlayerID 1-24)
- All devices connected to the same Wi-Fi network

## Development Conventions

### Networking Patterns
- Uses Unity Netcode for GameObjects with ServerRpc and ClientRpc for communication
- State synchronization using NetworkVariables
- Ownership-based object management for memes
- Server-authoritative architecture for meme state management

### Meme Placement Modes
- **World**: Freely positioned in 3D space with persistent position relative to anchor
- **Face**: Attached to face tracking anchors
- **Background**: Attached to background/environment anchors

### State Persistence
- Meme states saved to persistent data path per player/meme combination
- Quick selection preferences stored per player
- Network-transparent state loading for consistent experience

### Logging
- Centralized logging through the Logger component
- Network-aware logging that aggregates messages from all clients on the server
- File-based logging for debugging and analysis

## Key Features

### Meme Management
- Synchronized AR meme spawning across devices
- Persistent meme positioning using MR Utility Kit anchors
- Support for multiple meme types: 2D, 3D, Face, Video, and Environment memes

### User Interface
- Two-mode meme menu: Quick Selection and Group Mode
- Touch-free interaction using controller inputs
- Customizable quick selection that persists between sessions

### AR Integration
- Meta XR SDK integration for AR functionality
- MR Utility Kit for spatial mapping and anchor management
- Support for passthrough AR on Meta Quest devices

### Network Synchronization
- Real-time meme state synchronization
- Ownership transfer when spawning new memes
- Server-hosted authoritative state management
- Client-side position prediction and server reconciliation

## Project Structure

```
Assets/
├── Scenes/                    # Main scene (SampleScene.unity)
├── Scripts/                   # Core scripts (networking, UI, logging)
├── Resources/                 # Runtime-loaded content
├── Prefabs/                   # Reusable game objects
├── Memes/                     # Player-specific meme prefabs
├── Settings/                  # Project settings
├── InteractionSDK/            # Meta Interaction SDK
├── MetaXR/                    # Meta XR components
└── ...                       # Additional assets and configurations
```

## Special Considerations

- The host device should remain unworn as it functions solely as the network host
- Client devices are worn by participants for chatting and meme-sharing interactions
- Each PlayerID (1-24) corresponds to a unique set of memes for user study purposes
- Network state is managed server-authoritatively to prevent cheating/tampering
- Meme state is persisted locally per device and synchronized through the server