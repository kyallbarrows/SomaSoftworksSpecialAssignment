/**
 * Parser for Bible API JSON responses
 */

import { BibleChapterResponse, ChapterData, ChapterReference, MetaData } from './types';

/**
 * Validates if the given value is a valid ChapterReference
 */
function isChapterReference(value: any): value is ChapterReference {
  return (
    typeof value === 'object' &&
    value !== null &&
    typeof value.id === 'string' &&
    typeof value.number === 'string' &&
    typeof value.bookId === 'string'
  );
}

/**
 * Validates if the given value is valid ChapterData
 */
function isChapterData(value: any): value is ChapterData {
  return (
    typeof value === 'object' &&
    value !== null &&
    typeof value.id === 'string' &&
    typeof value.bibleId === 'string' &&
    typeof value.number === 'string' &&
    typeof value.bookId === 'string' &&
    typeof value.reference === 'string' &&
    typeof value.copyright === 'string' &&
    typeof value.verseCount === 'number' &&
    typeof value.content === 'string' &&
    (value.next === undefined || isChapterReference(value.next)) &&
    (value.previous === undefined || isChapterReference(value.previous))
  );
}

/**
 * Validates if the given value is valid MetaData
 */
function isMetaData(value: any): value is MetaData {
  return (
    typeof value === 'object' &&
    value !== null &&
    typeof value.fumsToken === 'string'
  );
}

/**
 * Validates if the given value is a valid BibleChapterResponse
 */
function isBibleChapterResponse(value: any): value is BibleChapterResponse {
  return (
    typeof value === 'object' &&
    value !== null &&
    isChapterData(value.data) &&
    isMetaData(value.meta)
  );
}

/**
 * Parses a JSON string or object into a BibleChapterResponse
 * @param input - JSON string or object to parse
 * @returns Parsed and validated BibleChapterResponse
 * @throws Error if the input is invalid
 */
export function parseBibleChapter(input: string | object): BibleChapterResponse {
  let parsed: any;

  // If input is a string, parse it as JSON
  if (typeof input === 'string') {
    try {
      parsed = JSON.parse(input);
    } catch (error) {
      throw new Error(`Invalid JSON: ${error instanceof Error ? error.message : String(error)}`);
    }
  } else {
    parsed = input;
  }

  // Validate the parsed data
  if (!isBibleChapterResponse(parsed)) {
    throw new Error('Invalid Bible chapter response structure');
  }

  return parsed;
}

/**
 * Safely parses a Bible chapter JSON, returning null on error
 * @param input - JSON string or object to parse
 * @returns Parsed BibleChapterResponse or null if invalid
 */
export function tryParseBibleChapter(input: string | object): BibleChapterResponse | null {
  try {
    return parseBibleChapter(input);
  } catch {
    return null;
  }
}
