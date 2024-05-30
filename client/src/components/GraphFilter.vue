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

  const showFilter = ref(false);

  const searchValue = ref('');

  const filterTree = ref(
    props.nodes
      .map(node => ({
        id: node.id,
        name: node.name,
        expand: false,
        show: true,
        fields: props.edgesMap[node.id].map(edge => ({
          id: edge.id,
          name: edge.name,
          show: true,
        })),
      }))
      .sort((a, b) => a.name.localeCompare(b.name))
  );

  const filteredFilterTree = computed(() => {
    const normalizedSearchValue = searchValue.value.toLowerCase();

    return filterTree.value
      .filter(node => {
        return (
          node.name.toLowerCase().includes(normalizedSearchValue) ||
          node.fields.some(field => field.name.toLowerCase().includes(normalizedSearchValue))
        );
      })
      .map(node => ({
        ...node,
        fields: node.fields.filter(field =>
          field.name.toLowerCase().includes(normalizedSearchValue)
        ),
      }));
  });

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

  function handleFilterButtonClick() {
    showFilter.value = !showFilter.value;
  }

  function handleNodeExpandButtonClick(nodeId: number) {
    const node = filterTree.value.find(node => node.id === nodeId);

    if (node === undefined) {
      return;
    }

    node.expand = !node.expand;
  }

  // TODO: I honestly don't know if this is
  // performant. Emitting an event for every
  // node and field...feels like maybe
  // there could be just filter all event
  // that is emitted once.
  function handleSelectAllButtonClick() {
    if (filterCount.value === 0) {
      filterTree.value.forEach(node => {
        node.show = false;
        node.fields.forEach(field => {
          field.show = false;
          emit('filter:edge', node.id, field.id, field.show);
        });
        emit('filter:node', node.id, node.show);
      });

      return;
    }

    filterTree.value.forEach(node => {
      node.show = true;
      node.fields.forEach(field => {
        field.show = true;
        emit('filter:edge', node.id, field.id, field.show);
      });
      emit('filter:node', node.id, node.show);
    });
  }

  function handleExpandAllButtonClick() {
    filterTree.value.forEach(node => (node.expand = true));
  }

  function handleCollapseAllButtonClick() {
    filterTree.value.forEach(node => (node.expand = false));
  }

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
    <div class="filter-toggle-container">
      <div>Filters ({{ filterCount }})</div>
      <button type="button" @click="handleFilterButtonClick">
        <span v-if="showFilter">
          <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 320 512" fill="currentColor">
            <path
              d="M182.6 137.4c-12.5-12.5-32.8-12.5-45.3 0l-128 128c-9.2 9.2-11.9 22.9-6.9 34.9s16.6 19.8 29.6 19.8H288c12.9 0 24.6-7.8 29.6-19.8s2.2-25.7-6.9-34.9l-128-128z"
            />
          </svg>
          <span class="sr-only">Close Filters</span>
        </span>
        <span v-else>
          <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 320 512" fill="currentColor">
            <path
              d="M137.4 374.6c12.5 12.5 32.8 12.5 45.3 0l128-128c9.2-9.2 11.9-22.9 6.9-34.9s-16.6-19.8-29.6-19.8L32 192c-12.9 0-24.6 7.8-29.6 19.8s-2.2 25.7 6.9 34.9l128 128z"
            />
          </svg>
          <span class="sr-only">Open Filters</span>
        </span>
      </button>
    </div>
    <div class="filter-list-container" v-if="showFilter">
      <div>
        <label for="searchValue" class="sr-only">Search</label>
        <input
          id="searchValue"
          name="searchValue"
          type="text"
          placeholder="Search"
          v-model="searchValue"
        />
      </div>
      <div>
        <div class="bulk-action-container">
          <div>
            <input
              id="selectAll"
              name="selectAll"
              type="checkbox"
              title="Select All"
              :checked="filterCount === 0"
              @click="handleSelectAllButtonClick"
            />
            <label for="selectAll" class="sr-only">Select All</label>
          </div>
          <button type="button" title="Expand All" @click="handleExpandAllButtonClick">
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 448 512" fill="currentColor">
              <path
                d="M256 80c0-17.7-14.3-32-32-32s-32 14.3-32 32V224H48c-17.7 0-32 14.3-32 32s14.3 32 32 32H192V432c0 17.7 14.3 32 32 32s32-14.3 32-32V288H400c17.7 0 32-14.3 32-32s-14.3-32-32-32H256V80z"
              />
            </svg>
            <span class="sr-only">Expand All</span>
          </button>
          <button type="button" title="Collapse All" @click="handleCollapseAllButtonClick">
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 448 512" fill="currentColor">
              <path
                d="M432 256c0 17.7-14.3 32-32 32L48 288c-17.7 0-32-14.3-32-32s14.3-32 32-32l352 0c17.7 0 32 14.3 32 32z"
              />
            </svg>
            <span class="sr-only">Collapse All</span>
          </button>
        </div>
        <ul class="filter-list">
          <li v-for="node in filteredFilterTree" :key="node.name" class="branch">
            <div class="filter-box">
              <input
                :id="node.name"
                :name="node.name"
                type="checkbox"
                :checked="node.show"
                @click="() => handleNodeClick(node.id)"
              />
              <div class="node-label-container">
                <button
                  v-if="node.fields.length > 0"
                  @click="() => handleNodeExpandButtonClick(node.id)"
                >
                  <span v-if="node.expand">
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      viewBox="0 0 320 512"
                      fill="currentColor"
                    >
                      <path
                        d="M182.6 137.4c-12.5-12.5-32.8-12.5-45.3 0l-128 128c-9.2 9.2-11.9 22.9-6.9 34.9s16.6 19.8 29.6 19.8H288c12.9 0 24.6-7.8 29.6-19.8s2.2-25.7-6.9-34.9l-128-128z"
                      />
                    </svg>
                    <span class="sr-only">Collapse</span>
                  </span>
                  <span v-else>
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      viewBox="0 0 320 512"
                      fill="currentColor"
                    >
                      <path
                        d="M137.4 374.6c12.5 12.5 32.8 12.5 45.3 0l128-128c9.2-9.2 11.9-22.9 6.9-34.9s-16.6-19.8-29.6-19.8L32 192c-12.9 0-24.6 7.8-29.6 19.8s-2.2 25.7 6.9 34.9l128 128z"
                      />
                    </svg>
                    <span class="sr-only">Expand</span>
                  </span>
                </button>
                <label :for="node.name" :title="node.name">{{ node.name }}</label>
              </div>
            </div>
            <ul class="filter-list" v-if="node.expand">
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

    & .filter-toggle-container {
      display: flex;
      justify-content: space-between;
      align-items: center;

      & button {
        display: flex;
        justify-content: center;
        align-items: center;
        background-color: inherit;
        border: none;
        color: inherit;
        cursor: pointer;

        & span {
          display: flex;
          justify-content: center;
          align-items: center;

          & svg {
            width: 1rem;
            height: 1rem;
          }
        }
      }
    }

    & .filter-list-container {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      padding: 0.25rem;
      overflow: auto;

      & #searchValue {
        width: 100%;
        padding: 0.25rem;
        border-radius: 0.25rem;

        &::placeholder {
          font-style: italic;
        }
      }

      & .bulk-action-container {
        display: flex;
        gap: 0.25rem;

        & input {
          cursor: pointer;
        }

        & button {
          display: flex;
          justify-content: center;
          align-items: center;
          background-color: inherit;
          border: none;
          color: inherit;
          cursor: pointer;
          padding: 0.25rem;

          & svg {
            width: 0.75rem;
            height: 0.75rem;
          }
        }
      }

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
        gap: 0.25rem;
        align-items: center;

        & label,
        & input {
          cursor: pointer;
        }

        & .node-label-container,
        & label {
          white-space: nowrap;
          overflow: hidden;
          text-overflow: ellipsis;
        }

        & .node-label-container {
          display: flex;
          gap: 0rem;
          align-items: center;

          & button {
            display: flex;
            justify-content: center;
            align-items: center;
            background-color: inherit;
            border: none;
            color: inherit;
            cursor: pointer;

            & span {
              display: flex;
              justify-content: center;
              align-items: center;

              & svg {
                width: 1rem;
                height: 1rem;
              }
            }
          }
        }
      }
    }
  }
</style>
