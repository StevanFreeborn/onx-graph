import { describe, expect, it } from 'vitest';
import { toTitleCase } from '../src/utils';

describe('utils', () => {
  describe('toTitleCase', () => {
    it('should convert to lower case words to title case', () => {
      const str = 'hello world';
      const result = toTitleCase(str);
      expect(result).toBe('Hello World');
    });

    it('should convert camel case word to title case', () => {
      const str = 'confirmPassword';
      const result = toTitleCase(str);
      expect(result).toBe('Confirm Password');
    });
  });
});
