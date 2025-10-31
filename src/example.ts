/**
 * Example usage of the Bible Chapter JSON parser
 */

import { parseBibleChapter, BibleChapterResponse } from './index';
import * as sampleData from './sample-data.json';

console.log('=== Bible Chapter JSON Parser Example ===\n');

try {
  // Parse the JSON data from the imported file
  const result: BibleChapterResponse = parseBibleChapter(sampleData);

  console.log('✓ Successfully parsed Bible chapter JSON\n');
  console.log('Chapter Information:');
  console.log('-------------------');
  console.log(`ID: ${result.data.id}`);
  console.log(`Reference: ${result.data.reference}`);
  console.log(`Bible ID: ${result.data.bibleId}`);
  console.log(`Book ID: ${result.data.bookId}`);
  console.log(`Chapter Number: ${result.data.number}`);
  console.log(`Verse Count: ${result.data.verseCount}`);
  console.log(`\nCopyright: ${result.data.copyright.substring(0, 100)}...`);
  
  if (result.data.previous) {
    console.log(`\nPrevious Chapter: ${result.data.previous.id} (Chapter ${result.data.previous.number})`);
  }
  
  if (result.data.next) {
    console.log(`Next Chapter: ${result.data.next.id} (Chapter ${result.data.next.number})`);
  }

  console.log(`\nContent Length: ${result.data.content.length} characters`);
  console.log(`Content Preview: ${result.data.content.substring(0, 150)}...`);
  
  console.log(`\nMeta Information:`);
  console.log(`FUMS Token: ${result.meta.fumsToken.substring(0, 50)}...`);

} catch (error) {
  console.error('✗ Failed to parse JSON:', error instanceof Error ? error.message : String(error));
  process.exit(1);
}

console.log('\n=== Parser Test Complete ===');
