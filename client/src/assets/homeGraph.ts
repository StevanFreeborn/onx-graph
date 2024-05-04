export const homeGraph = {
  nodes: {
    node1: { name: 'Node 5' },
    node2: { name: 'Node 2' },
    node3: { name: 'Node 3' },
    node4: { name: 'Node 4' },
  },
  edges: {
    edge1: { source: 'node1', target: 'node2' },
    edge2: { source: 'node2', target: 'node3' },
    edge3: { source: 'node3', target: 'node4' },
  },
};

export const exampleGraph = {
  id: '66328733d353c9c99d85acc4',
  name: 'IDM15',
  createdAt: '2024-05-01T18:17:23.56Z',
  updatedAt: '2024-05-01T18:17:28.616Z',
  status: 2,
  nodes: [
    {
      id: 2,
      name: 'Users',
    },
    {
      id: 1,
      name: 'Roles',
    },
    {
      id: 3,
      name: 'Groups',
    },
  ],
  edgesMap: {
    '1': [
      {
        multiplicity: 1,
        referencedAppId: 3,
        id: 23,
        appId: 1,
        name: 'Groups',
        type: 500,
        status: 0,
        isRequired: false,
        isUnique: false,
      },
      {
        multiplicity: 1,
        referencedAppId: 3,
        id: 999,
        appId: 1,
        name: 'Groups',
        type: 500,
        status: 0,
        isRequired: false,
        isUnique: false,
      },
      {
        multiplicity: 1,
        referencedAppId: 3,
        id: 9999,
        appId: 1,
        name: 'Groups',
        type: 500,
        status: 0,
        isRequired: false,
        isUnique: false,
      },
      {
        multiplicity: 1,
        referencedAppId: 3,
        id: 99999,
        appId: 1,
        name: 'Groups',
        type: 500,
        status: 0,
        isRequired: false,
        isUnique: false,
      },
      {
        multiplicity: 1,
        referencedAppId: 3,
        id: 999999,
        appId: 1,
        name: 'Groups',
        type: 500,
        status: 0,
        isRequired: false,
        isUnique: false,
      },
      {
        multiplicity: 1,
        referencedAppId: 3,
        id: 1111111,
        appId: 1,
        name: 'Groups',
        type: 500,
        status: 0,
        isRequired: false,
        isUnique: false,
      },
      {
        multiplicity: 1,
        referencedAppId: 3,
        id: 111111,
        appId: 1,
        name: 'Groups',
        type: 500,
        status: 0,
        isRequired: false,
        isUnique: false,
      },
      {
        multiplicity: 0,
        referencedAppId: 2,
        id: 41,
        appId: 1,
        name: 'Updated By',
        type: 500,
        status: 0,
        isRequired: false,
        isUnique: false,
      },
      {
        multiplicity: 1,
        referencedAppId: 2,
        id: 31,
        appId: 1,
        name: 'Users',
        type: 500,
        status: 0,
        isRequired: false,
        isUnique: false,
      },
      {
        multiplicity: 0,
        referencedAppId: 2,
        id: 44,
        appId: 1,
        name: 'Created By',
        type: 500,
        status: 0,
        isRequired: false,
        isUnique: false,
      },
      {
        multiplicity: 0,
        referencedAppId: 2,
        id: 48,
        appId: 1,
        name: 'Last Saved By',
        type: 500,
        status: 0,
        isRequired: false,
        isUnique: false,
      },
    ],
    '2': [
      {
        multiplicity: 1,
        referencedAppId: 3,
        id: 20,
        appId: 2,
        name: 'Groups',
        type: 500,
        status: 0,
        isRequired: false,
        isUnique: false,
      },
      {
        multiplicity: 0,
        referencedAppId: 2,
        id: 50,
        appId: 2,
        name: 'Last Saved By',
        type: 500,
        status: 0,
        isRequired: false,
        isUnique: false,
      },
      {
        multiplicity: 1,
        referencedAppId: 1,
        id: 30,
        appId: 2,
        name: 'Roles',
        type: 500,
        status: 0,
        isRequired: false,
        isUnique: false,
      },
      {
        multiplicity: 0,
        referencedAppId: 2,
        id: 895,
        appId: 2,
        name: 'Manager',
        type: 500,
        status: 0,
        isRequired: false,
        isUnique: false,
      },
      {
        multiplicity: 1,
        referencedAppId: 2,
        id: 896,
        appId: 2,
        name: 'Direct Reports',
        type: 500,
        status: 0,
        isRequired: false,
        isUnique: false,
      },
      {
        multiplicity: 0,
        referencedAppId: 2,
        id: 42,
        appId: 2,
        name: 'Created By',
        type: 500,
        status: 0,
        isRequired: false,
        isUnique: false,
      },
      {
        multiplicity: 0,
        referencedAppId: 2,
        id: 39,
        appId: 2,
        name: 'Updated By',
        type: 500,
        status: 0,
        isRequired: false,
        isUnique: false,
      },
    ],
    '3': [
      {
        multiplicity: 1,
        referencedAppId: 1,
        id: 22,
        appId: 3,
        name: 'Roles',
        type: 500,
        status: 0,
        isRequired: false,
        isUnique: false,
      },
      {
        multiplicity: 0,
        referencedAppId: 2,
        id: 52,
        appId: 3,
        name: 'Last Saved By',
        type: 500,
        status: 0,
        isRequired: false,
        isUnique: false,
      },
      {
        multiplicity: 0,
        referencedAppId: 2,
        id: 43,
        appId: 3,
        name: 'Created By',
        type: 500,
        status: 0,
        isRequired: false,
        isUnique: false,
      },
      {
        multiplicity: 1,
        referencedAppId: 2,
        id: 21,
        appId: 3,
        name: 'Users',
        type: 500,
        status: 0,
        isRequired: false,
        isUnique: false,
      },
      {
        multiplicity: 1,
        referencedAppId: 3,
        id: 18,
        appId: 3,
        name: 'Parent Groups',
        type: 500,
        status: 0,
        isRequired: false,
        isUnique: false,
      },
      {
        multiplicity: 1,
        referencedAppId: 3,
        id: 19,
        appId: 3,
        name: 'Child Groups',
        type: 500,
        status: 0,
        isRequired: false,
        isUnique: false,
      },
      {
        multiplicity: 0,
        referencedAppId: 2,
        id: 40,
        appId: 3,
        name: 'Updated By',
        type: 500,
        status: 0,
        isRequired: false,
        isUnique: false,
      },
    ],
  },
};
