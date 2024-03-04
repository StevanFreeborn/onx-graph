import { mount } from '@vue/test-utils';
import { describe, expect, it } from 'vitest';
import { defineComponent } from 'vue';
import { useSubmitting } from '../../src/composables/useSubmitting';

describe('useSubmitting', () => {
  const TestComponent = defineComponent({
    template: '<div></div>',
    setup() {
      const result = useSubmitting();
      return { ...result };
    },
  });

  it('should expose a boolean value', () => {
    const wrapper = mount(TestComponent);
    expect(wrapper.vm.isSubmitting).toBeDefined();
    expect(wrapper.vm.isSubmitting).toBeTypeOf('boolean');
  });

  it('should expose a function to set isSubmitting', () => {
    const wrapper = mount(TestComponent);
    expect(wrapper.vm.setIsSubmitting).toBeDefined();
    expect(wrapper.vm.setIsSubmitting).toBeTypeOf('function');
  });

  describe('setIsSubmitting', () => {
    it('should set isSubmitting to true', () => {
      const wrapper = mount(TestComponent);
      wrapper.vm.setIsSubmitting(true);
      expect(wrapper.vm.isSubmitting).toBe(true);
    });

    it('should set isSubmitting to false', () => {
      const wrapper = mount(TestComponent);
      wrapper.vm.setIsSubmitting(false);
      expect(wrapper.vm.isSubmitting).toBe(false);
    });
  });
});
