# Bible Chapter JSON Parser (TypeScript)

A TypeScript parser for Bible API JSON responses with type-safe interfaces and validation.

## Features

- ✅ Strongly typed TypeScript interfaces
- ✅ JSON parsing and validation
- ✅ Support for Bible chapter data structure
- ✅ Safe parsing with error handling
- ✅ Next/Previous chapter references

## Installation

```bash
npm install
```

## Usage

### Basic Usage

```typescript
import { parseBibleChapter, BibleChapterResponse } from './src/index';

const jsonString = '{"data":{"id":"1CH.3",...},"meta":{...}}';
const result: BibleChapterResponse = parseBibleChapter(jsonString);

console.log(result.data.reference); // "1 Chronicles 3"
console.log(result.data.verseCount); // 24
```

### Safe Parsing

```typescript
import { tryParseBibleChapter } from './src/index';

const result = tryParseBibleChapter(jsonString);
if (result) {
  console.log('Successfully parsed:', result.data.reference);
} else {
  console.log('Failed to parse JSON');
}
```

## Data Structure

The parser handles the following JSON structure:

```typescript
interface BibleChapterResponse {
  data: {
    id: string;                    // Chapter ID (e.g., "1CH.3")
    bibleId: string;               // Bible version ID
    number: string;                // Chapter number
    bookId: string;                // Book ID (e.g., "1CH")
    reference: string;             // Human-readable reference
    copyright: string;             // Copyright information
    verseCount: number;            // Number of verses
    content: string;               // HTML content with verses
    next?: ChapterReference;       // Next chapter reference
    previous?: ChapterReference;   // Previous chapter reference
  };
  meta: {
    fumsToken: string;             // FUMS tracking token
  };
}
```

## Building

Compile TypeScript to JavaScript:

```bash
npm run build
```

## Testing

Run the example parser:

```bash
npm test
```

This will parse the provided Bible chapter JSON and display the extracted information.

## API Reference

### `parseBibleChapter(input: string | object): BibleChapterResponse`

Parses and validates a Bible chapter JSON string or object.

- **Parameters:**
  - `input`: JSON string or object to parse
- **Returns:** Validated `BibleChapterResponse` object
- **Throws:** Error if the input is invalid or doesn't match the expected structure

### `tryParseBibleChapter(input: string | object): BibleChapterResponse | null`

Safely parses a Bible chapter JSON, returning `null` on error instead of throwing.

- **Parameters:**
  - `input`: JSON string or object to parse
- **Returns:** `BibleChapterResponse` object or `null` if parsing fails

## Types Exported

- `BibleChapterResponse` - Complete response structure
- `ChapterData` - Main chapter data
- `ChapterReference` - Next/Previous chapter reference
- `MetaData` - Metadata with FUMS token

## Example JSON

The parser is designed for JSON responses from Bible APIs with this structure:

```json
{
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
}
```

## License

ISC
