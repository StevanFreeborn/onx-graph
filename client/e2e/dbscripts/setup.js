// Create a new user for testing
db.users.insertOne({
  _id: ObjectId().toString(),
  email: 'test@test.com',
  username: 'testUser',
  password: '$2a$11$Xs1mALyCfYD7Er2542tlVupp7GnXIwj5kA/0e6d1Dapws80QwuWoq',
  createdAt: new Date(),
  updatedAt: new Date(),
  isVerified: true,
});
