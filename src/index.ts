/**
 * Bible Chapter JSON Parser
 * 
 * This module provides TypeScript interfaces and parsing utilities
 * for Bible API JSON responses.
 */

export { 
  BibleChapterResponse, 
  ChapterData, 
  ChapterReference, 
  MetaData 
} from './types';

export { 
  parseBibleChapter, 
  tryParseBibleChapter 
} from './parser';
