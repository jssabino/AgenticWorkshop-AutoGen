# AutoGen Studio Setup Guide

## Installation Steps

1. **Install Python 3.8+**
   ```bash
   python --version
   ```

2. **Install AutoGen Studio**
   ```bash
   pip install autogenstudio
   ```

3. **Launch AutoGen Studio**
   ```bash
   autogenstudio ui --port 8081
   ```

4. **Open in Browser**
   Navigate to `http://localhost:8081`

## Configuration

1. **Set up API Keys**
   - Navigate to Settings → Models
   - Add your OpenAI API key
   - Or configure Azure OpenAI settings

2. **Import Agent Configurations**
   - Go to Agents section
   - Click "Import Agent"
   - Upload the JSON files from the `agents/` folder

3. **Import Workflows**
   - Go to Workflows section
   - Click "Import Workflow"
   - Upload the JSON files from the `workflows/` folder

## Quick Start

1. **Test Basic Agent**
   - Select an agent from the Agents list
   - Click "Test Agent"
   - Start a conversation

2. **Run Multi-Agent Workflow**
   - Select a workflow from the Workflows list
   - Click "Start Workflow"
   - Follow the conversation flow

## Troubleshooting

### Common Issues

1. **Port Already in Use**
   ```bash
   autogenstudio ui --port 8082
   ```

2. **API Key Issues**
   - Check API key format
   - Verify API key permissions
   - Test with simple agent first

3. **Model Not Found**
   - Verify model name in configuration
   - Check model availability in your region

### Getting Help

- Check the AutoGen Studio documentation
- Join the AutoGen Discord community
- Review the GitHub issues page
