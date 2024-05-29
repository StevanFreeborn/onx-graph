<script setup lang="ts">
  import type { AppNode, FieldEdge } from '@/types';
  import { computed, ref } from 'vue';

  const props = defineProps<{
    nodes: AppNode[];
    edgesMap: Record<string, FieldEdge[]>;
  }>();

  const emit = defineEmits<{
    'filter:node': [nodeId: number, show: boolean];
    'filter:edge': [nodeId: number, fieldId: number, show: boolean];
  }>();

  const filterTree = ref(
    props.nodes
      .map(node => ({
        id: node.id,
        name: node.name,
        show: true,
        fields: props.edgesMap[node.id].map(edge => ({
          id: edge.id,
          name: edge.name,
          show: true,
        })),
      }))
      .sort((a, b) => a.name.localeCompare(b.name))
  );

  const filterCount = computed(() => {
    let count = 0;

    for (const node of filterTree.value) {
      if (node.show === false) {
        count += 1;
      }

      for (const field of node.fields) {
        if (field.show === false) {
          count += 1;
        }
      }
    }

    return count;
  });

  function handleNodeClick(nodeId: number) {
    const node = filterTree.value.find(node => node.id === nodeId);

    if (node === undefined) {
      return;
    }

    node.show = !node.show;
    node.fields.forEach(field => (field.show = node.show));

    emit('filter:node', node.id, node.show);
  }

  function handleFieldClick(fieldId: number) {
    const node = filterTree.value.find(node => node.fields.some(field => field.id === fieldId));

    if (node === undefined) {
      return;
    }

    const field = node.fields.find(field => field.id === fieldId);

    if (field === undefined) {
      return;
    }

    field.show = !field.show;

    emit('filter:edge', node.id, field.id, field.show);
  }
</script>

<template>
  <div class="filter-container">
    <div>
      <div>Filter ({{ filterCount }})</div>
      <button></button>
    </div>
    <div>
      <label>Search</label>
      <input type="text" />
    </div>
    <div>
      <ul class="filter-list">
        <li v-for="node in filterTree" :key="node.name" class="branch">
          <div class="filter-box">
            <input
              :id="node.name"
              :name="node.name"
              type="checkbox"
              :checked="node.show"
              @click="() => handleNodeClick(node.id)"
            />
            <label :for="node.name" :title="node.name">{{ node.name }}</label>
          </div>
          <ul class="filter-list">
            <li v-for="field in node.fields" :key="field.name" class="leaf">
              <div class="filter-box">
                <input
                  :id="field.name"
                  :name="field.name"
                  type="checkbox"
                  :checked="field.show"
                  @click="() => handleFieldClick(field.id)"
                />
                <label :for="field.name" title="field.name">{{ field.name }}</label>
              </div>
            </li>
          </ul>
        </li>
      </ul>
    </div>
  </div>
</template>

<style scoped>
  .filter-container {
    padding: 0.75rem 1rem 0.75rem 1rem;
    background-color: var(--color-background);
    border-radius: 0.5rem;
    box-shadow: 0 2px 4px 0 rgba(0, 0, 0, 0.1);
    overflow: hidden;
    max-width: 250px;
    display: flex;
    flex-direction: column;
    gap: 0.25rem;
    max-height: 300px;
    overflow: auto;

    & .filter-list {
      & .branch {
        display: flex;
        flex-direction: column;
        gap: 0.25rem;

        & .leaf {
          display: flex;
          flex-direction: column;
          gap: 0.25rem;
          padding-left: 1.5rem;
        }
      }
    }

    & .filter-box {
      display: flex;
      gap: 0.5rem;
      align-items: center;

      & label,
      & input {
        cursor: pointer;
      }

      & label {
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
      }
    }
  }
</style>
