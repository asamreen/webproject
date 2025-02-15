const About = () => {
  return (
    <>
      <div className="flex flex-wrap gap-2 sm:gap-x-6 items-center justify-center">
        <h1 className="text-4xl font-bold leading-none tracking-tight sm:text-6xl">
          We love
        </h1>
        <div className="stats bg-primary shadow">
          <div className="stat">
            <div className="stat-title text-primary-content text-4xl font-bold tracking-widest">
              Store
            </div>
          </div>
        </div>
      </div>
      <p className="mt-6 text-lg leading-8 max-w-2xl mx-auto">
        We aim to connect people with quality products that bring value and
        convenience to their lives. At Store, we believe in building a
        community of happy and satisfied customers. Thank you for choosing Store. Let us make your shopping experience better than ever!
      </p>
    </>
  );
};
export default About;
