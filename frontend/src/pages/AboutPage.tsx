export default function AboutPage() {
  const teamMembers = [
    {
      name: 'Diko Mohamed',
      role: 'Project Manager & Developer',
      image: 'https://prospergenics.com/wp-content/uploads/2026/02/diko_profile-300x300.png',
    },
    {
      name: 'Simitia Mpoe',
      role: 'Programming & Leadership',
      image: 'https://prospergenics.com/wp-content/uploads/2026/02/sandra_profile-300x300.png',
    },
    {
      name: 'Lessiamon Mpoe',
      role: 'React Developer',
      image: 'https://prospergenics.com/wp-content/uploads/2026/02/lessy_profile-300x300.png',
    },
    {
      name: 'Frank Kobaai',
      role: 'Web Developer',
      image: 'https://prospergenics.com/wp-content/uploads/2026/02/frank_profile-300x300.png',
    },
  ];

  return (
    <div>
      {/* Full-page Hero Section with Team Photo */}
      <div
        className="relative h-screen bg-cover bg-center bg-no-repeat flex items-center justify-center"
        style={{
          backgroundImage: "url('https://prospergenics.com/wp-content/themes/prospergenics-theme/images/hero-bg.jpg')",
        }}
      >
        {/* Dark overlay for text readability */}
        <div className="absolute inset-0 bg-black/50"></div>

        {/* Content */}
        <div className="relative z-10 text-center text-white px-6">
          <div className="mb-8">
            <img
              src="https://prospergenics.com/wp-content/uploads/2026/02/logo_header.png"
              alt="Prospergenics"
              className="h-32 mx-auto drop-shadow-2xl brightness-0 invert"
            />
          </div>
          <h1 className="text-6xl md:text-7xl font-bold mb-6 drop-shadow-lg">
            Prospergenics Learning Platform
          </h1>
          <p className="text-2xl md:text-3xl max-w-4xl mx-auto leading-relaxed mb-8 drop-shadow-lg">
            A community where members and coaches support each other to realize their potential
            and create opportunities together.
          </p>
          <a
            href="#mission"
            className="inline-block px-8 py-4 bg-gradient-to-r from-green-600 to-green-500 text-white text-xl font-bold rounded-full hover:shadow-2xl transition-all hover:-translate-y-1"
          >
            Learn More ↓
          </a>
        </div>
      </div>

      {/* Rest of content */}
      <div className="max-w-7xl mx-auto px-6 py-16">

      {/* Mission Statement */}
      <div id="mission" className="bg-gradient-to-r from-green-50 to-green-100 rounded-3xl p-12 mb-16 shadow-lg border-2 border-green-200">
        <h2 className="text-3xl font-bold text-green-800 mb-6 text-center">Our Mission</h2>
        <p className="text-lg text-gray-700 mb-6 leading-relaxed">
          We believe in <strong>practical action over theory</strong>. Our learning platform helps people
          in rural Africa develop real technology skills and build actual businesses through mentorship
          and hands-on learning.
        </p>
        <p className="text-lg text-gray-700 leading-relaxed">
          This platform is designed specifically for people with <strong>no formal education</strong>,
          teaching from the very basics to professional software development—step by step,
          with patience and encouragement.
        </p>
      </div>

      {/* What We Do */}
      <div className="mb-16">
        <h2 className="text-3xl font-bold text-gray-900 mb-8 text-center">What We Do</h2>
        <div className="grid md:grid-cols-3 gap-8">
          <div className="bg-white rounded-2xl p-8 shadow-lg hover:shadow-xl transition-shadow border-t-4 border-green-500">
            <div className="text-5xl mb-4">💻</div>
            <h3 className="text-2xl font-bold text-green-700 mb-4">Digital Technology</h3>
            <p className="text-gray-700 leading-relaxed">
              Teaching AI, software development, and modern tech tools to create opportunities
              in the digital economy.
            </p>
          </div>
          <div className="bg-white rounded-2xl p-8 shadow-lg hover:shadow-xl transition-shadow border-t-4 border-green-500">
            <div className="text-5xl mb-4">💰</div>
            <h3 className="text-2xl font-bold text-green-700 mb-4">Microcredit Access</h3>
            <p className="text-gray-700 leading-relaxed">
              Providing small loans for equipment and agricultural investments to kickstart
              sustainable businesses.
            </p>
          </div>
          <div className="bg-white rounded-2xl p-8 shadow-lg hover:shadow-xl transition-shadow border-t-4 border-green-500">
            <div className="text-5xl mb-4">🌍</div>
            <h3 className="text-2xl font-bold text-green-700 mb-4">Cultural Exchange</h3>
            <p className="text-gray-700 leading-relaxed">
              Facilitating cross-cultural learning and innovation between Africa and
              the global community.
            </p>
          </div>
        </div>
      </div>

      {/* Impact Stats */}
      <div className="bg-gradient-to-r from-green-600 to-green-500 rounded-3xl p-12 mb-16 text-white shadow-2xl">
        <h2 className="text-3xl font-bold mb-8 text-center">Our Impact</h2>
        <div className="grid md:grid-cols-3 gap-8 text-center">
          <div>
            <div className="text-5xl font-bold mb-2">15+</div>
            <div className="text-xl">Active Members</div>
          </div>
          <div>
            <div className="text-5xl font-bold mb-2">€100K+</div>
            <div className="text-xl">Community Wealth</div>
          </div>
          <div>
            <div className="text-5xl font-bold mb-2">25+</div>
            <div className="text-xl">Trainings Completed</div>
          </div>
        </div>
      </div>

      {/* Team Section */}
      <div className="mb-16">
        <h2 className="text-3xl font-bold text-gray-900 mb-8 text-center">Prospergenics Development Team</h2>
        <p className="text-lg text-gray-700 text-center mb-12 max-w-2xl mx-auto">
          Our diverse team of coaches and developers work together to empower learners
          and create lasting change.
        </p>
        <div className="grid md:grid-cols-4 gap-6">
          {teamMembers.map((member) => (
            <div
              key={member.name}
              className="bg-white rounded-2xl overflow-hidden shadow-lg hover:shadow-2xl transition-all hover:-translate-y-1 border-2 border-green-100"
            >
              <div className="aspect-square bg-gradient-to-br from-green-100 to-green-200 flex items-center justify-center">
                <img
                  src={member.image}
                  alt={member.name}
                  className="w-full h-full object-cover"
                  onError={(e) => {
                    // Fallback to placeholder if image doesn't load
                    e.currentTarget.src = `https://ui-avatars.com/api/?name=${encodeURIComponent(member.name)}&size=400&background=22c55e&color=fff&bold=true`;
                  }}
                />
              </div>
              <div className="p-6 text-center">
                <h3 className="text-xl font-bold text-gray-900 mb-2">{member.name}</h3>
                <p className="text-green-600 font-medium">{member.role}</p>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Call to Action */}
      <div className="text-center bg-white rounded-3xl p-12 shadow-xl border-2 border-green-200">
        <h2 className="text-3xl font-bold text-gray-900 mb-6">
          Ready to Start Your Learning Journey?
        </h2>
        <p className="text-lg text-gray-700 mb-8">
          Join our community and learn valuable technology skills that create real opportunities.
        </p>
        <a
          href="/chat"
          className="inline-block px-8 py-4 bg-gradient-to-r from-green-600 to-green-500 text-white text-lg font-bold rounded-full hover:shadow-lg transition-all hover:-translate-y-0.5"
        >
          Start Learning Now →
        </a>
      </div>
      </div>
    </div>
  );
}
