import os
import autogen
from autogen import AssistantAgent, UserProxyAgent
from dotenv import load_dotenv
load_dotenv()

llm_config = {
    "config_list": [
        {
            "model": "gpt-4",
            "base_url": os.environ["AZURE_OPENAI_ENDPOINT"],
            "api_type": "azure",
            "api_version": os.environ["AZURE_OPENAI_API_VERSION"],
            "max_tokens": os.environ["MAX_TOKENS"],
            "api_key": os.environ["AZURE_OPENAI_API_KEY"]
        }
    ],
}
assistant = AssistantAgent("assistant", llm_config=llm_config)

user_proxy = UserProxyAgent(
    "user_proxy", code_execution_config={"executor": autogen.coding.LocalCommandLineCodeExecutor(work_dir="coding")}
)

# Start the chat
user_proxy.initiate_chat(
    assistant,
    message="Plot a chart of NVDA and TESLA stock price change YTD. and save the chart as a PNG file.",
)