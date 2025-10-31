/**
 * TypeScript interfaces for Bible API JSON response
 */

/**
 * Reference to a chapter (next or previous)
 */
export interface ChapterReference {
  id: string;
  number: string;
  bookId: string;
}

/**
 * Main chapter data structure
 */
export interface ChapterData {
  id: string;
  bibleId: string;
  number: string;
  bookId: string;
  reference: string;
  copyright: string;
  verseCount: number;
  content: string;
  next?: ChapterReference;
  previous?: ChapterReference;
}

/**
 * Metadata structure
 */
export interface MetaData {
  fumsToken: string;
}

/**
 * Complete Bible chapter response structure
 */
export interface BibleChapterResponse {
  data: ChapterData;
  meta: MetaData;
}
