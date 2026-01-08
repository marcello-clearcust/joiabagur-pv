# Add Conversation to Prompts

Add conversation to Documentos/prompts.md using interactive process with LLM.

## Usage

cursor add-prompts

## Description

This command starts interactive process with LLM. When you run it, tell the LLM:

"I want to add a conversation to the prompts.md file following specific rules"

## Process

1. LLM will ask for the title
2. Will request you to paste the complete conversation
3. Will process automatically and add to file

## Rules

- Never cut user prompts - keep all original content
- Summarize assistant responses (except complete questions)
- Translate to Spanish if necessary
- Format: ## Prompt about [Title] + --- + Response + ---

## Example

cursor add-prompts

Then: "I want to add the conversation about testing to the prompts.md file"
