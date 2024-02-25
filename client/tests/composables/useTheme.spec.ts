import { mount } from '@vue/test-utils';
import { Mock, afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { useTheme } from '../../src/composables/useTheme';

describe('useTheme', () => {
  const TestComponent = {
    template: '<div></div>',
    setup() {
      const theme = useTheme();
      return { theme };
    },
  };

  let matchMediaMock: Mock;
  let mediaQueryListMock: {
    matches: boolean;
    addEventListener: Mock;
    removeEventListener: Mock;
  };

  beforeEach(() => {
    mediaQueryListMock = {
      matches: false,
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
    };
    matchMediaMock = vi.fn().mockReturnValue(mediaQueryListMock);
    Object.defineProperty(window, 'matchMedia', { value: matchMediaMock });
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  it('should initialize with light theme', () => {
    const wrapper = mount(TestComponent);

    expect(wrapper.vm.theme).toBe('light');
  });

  it('should initialize with dark theme if media query matches', async () => {
    mediaQueryListMock.matches = true;

    const wrapper = mount(TestComponent);

    expect(wrapper.vm.theme).toBe('dark');
  });

  it('should update theme on media query change', async () => {
    const wrapper = mount(TestComponent);

    mediaQueryListMock.addEventListener.mock.calls[0][1]({ matches: true });

    expect(wrapper.vm.theme).toBe('dark');
  });

  it('should remove event listener on unmount', async () => {
    const wrapper = mount(TestComponent);

    wrapper.unmount();

    expect(mediaQueryListMock.removeEventListener).toHaveBeenCalled();
  });
});
