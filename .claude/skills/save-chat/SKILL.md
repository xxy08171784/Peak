---
name: save-chat
description: Save the current conversation transcript to a local markdown file.
---

# Save Chat

Save the current Claude Code conversation to a local markdown file with a timestamped filename.

## Instructions

When the user invokes `/save-chat`, do the following:

1. Ask the user for an optional short description/summary to include in the filename (e.g., "fixed-localization-bug"). If they don't provide one, use "chat" as default.

2. Create the directory `chats/` in the project root if it doesn't already exist.

3. Generate a filename: `chats/YYYY-MM-DD_HH-MM-SS_{description}.md` with the current timestamp.

4. Compile the conversation into a well-formatted markdown document containing:
   - A title (`# Conversation: {description}`)
   - Timestamp of when it was saved
   - A summary of the main topics discussed and decisions made
   - A "Changes Made" section listing all files modified/created and what changed
   - A "Key Decisions" section highlighting important architectural choices
   - A "Follow-up Notes" section with any pending tasks or next steps

5. Write the markdown file using the Write tool.

6. Report the file path to the user.

## Important

This should create a human-readable summary document, not a raw dump. Focus on the substance: what was decided, what was done, and what's next. This is for future reference when the user comes back to this project.
