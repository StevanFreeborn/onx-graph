import { mount } from '@vue/test-utils';
import { describe, expect, it } from 'vitest';
import { defineComponent } from 'vue';
import { useMounted } from '../../src/composables/useMounted';

describe('useMounted', () => {
  const TestComponent = defineComponent({
    template: '<div></div>',
    setup() {
      const isMounted = useMounted();
      return { isMounted };
    },
  });

  it('should expose a boolean value', () => {
    const wrapper = mount(TestComponent);
    expect(wrapper.vm.isMounted).toBeDefined();
    expect(wrapper.vm.isMounted).toBeTypeOf('boolean');
  });

  it('should return true when the component is mounted', async () => {
    const wrapper = mount(TestComponent);
    expect(wrapper.vm.isMounted).toBe(true);
  });

  it('should return false when the component is unmounted', () => {
    const wrapper = mount(TestComponent);

    wrapper.unmount();

    expect(wrapper.vm.isMounted).toBe(false);
  });
});
