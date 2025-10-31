# Usage Examples

## Example 1: Parse from JSON String

```typescript
import { parseBibleChapter } from './src/index';

const jsonString = `{
  "data": {
    "id": "1CH.3",
    "bibleId": "bba9f40183526463-01",
    "number": "3",
    "bookId": "1CH",
    "reference": "1 Chronicles 3",
    "copyright": "...",
    "verseCount": 24,
    "content": "<p>...</p>",
    "next": {
      "id": "1CH.4",
      "number": "4",
      "bookId": "1CH"
    },
    "previous": {
      "id": "1CH.2",
      "number": "2",
      "bookId": "1CH"
    }
  },
  "meta": {
    "fumsToken": "..."
  }
}`;

const result = parseBibleChapter(jsonString);
console.log(result.data.reference); // "1 Chronicles 3"
console.log(result.data.verseCount); // 24
```

## Example 2: Parse from Object

```typescript
import { parseBibleChapter } from './src/index';

const data = {
  data: {
    id: "1CH.3",
    bibleId: "bba9f40183526463-01",
    number: "3",
    bookId: "1CH",
    reference: "1 Chronicles 3",
    copyright: "...",
    verseCount: 24,
    content: "<p>...</p>",
    next: { id: "1CH.4", number: "4", bookId: "1CH" },
    previous: { id: "1CH.2", number: "2", bookId: "1CH" }
  },
  meta: {
    fumsToken: "..."
  }
};

const result = parseBibleChapter(data);
console.log(`Reading: ${result.data.reference}`);
```

## Example 3: Safe Parsing (No Exceptions)

```typescript
import { tryParseBibleChapter } from './src/index';

const jsonString = '{"invalid": "data"}';
const result = tryParseBibleChapter(jsonString);

if (result) {
  console.log('Successfully parsed:', result.data.reference);
} else {
  console.log('Failed to parse - invalid structure');
}
```

## Example 4: Type-Safe Access

```typescript
import { parseBibleChapter, BibleChapterResponse } from './src/index';

function displayChapter(response: BibleChapterResponse) {
  const { data, meta } = response;
  
  console.log(`Chapter: ${data.reference}`);
  console.log(`Verses: ${data.verseCount}`);
  console.log(`Bible: ${data.bibleId}`);
  
  if (data.previous) {
    console.log(`Previous: Chapter ${data.previous.number}`);
  }
  
  if (data.next) {
    console.log(`Next: Chapter ${data.next.number}`);
  }
  
  console.log(`Token: ${meta.fumsToken.substring(0, 20)}...`);
}

const jsonString = '...'; // Your JSON here
const response = parseBibleChapter(jsonString);
displayChapter(response);
```

## Example 5: Error Handling

```typescript
import { parseBibleChapter } from './src/index';

try {
  const result = parseBibleChapter(invalidJson);
  console.log('Parsed successfully:', result);
} catch (error) {
  if (error instanceof Error) {
    if (error.message.includes('Invalid JSON')) {
      console.error('The input is not valid JSON');
    } else if (error.message.includes('Invalid Bible chapter response')) {
      console.error('The JSON structure does not match the expected format');
    }
  }
}
```

## Example 6: Extract Specific Data

```typescript
import { parseBibleChapter } from './src/index';

const result = parseBibleChapter(jsonString);

// Extract just the chapter info
const chapterInfo = {
  id: result.data.id,
  reference: result.data.reference,
  verseCount: result.data.verseCount,
};

// Extract navigation
const navigation = {
  previous: result.data.previous?.id,
  current: result.data.id,
  next: result.data.next?.id,
};

console.log('Chapter:', chapterInfo);
console.log('Navigation:', navigation);
```

## Running the Example

To run the included example that demonstrates the parser:

```bash
npm install
npm test
```

This will parse the sample Bible chapter JSON and display all extracted information.
