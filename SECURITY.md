# Security Policy

## ?? Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| 1.0.x   | :white_check_mark: |
| < 1.0   | :x:                |

## ??? Reporting a Vulnerability

We take security seriously. If you discover a security vulnerability, please follow these steps:

### ?? Do NOT
- Open a public GitHub issue
- Discuss publicly in forums/chat
- Share exploit code publicly

### ? Do
1. **Email:** Send details to [your-email@example.com]
2. **Include:**
   - Description of the vulnerability
   - Steps to reproduce
   - Potential impact
   - Suggested fix (if any)

3. **Response Time:**
   - Initial response: Within 48 hours
   - Status update: Within 7 days
   - Fix timeline: Depends on severity

### Severity Levels

**Critical** (< 24 hours)
- Remote code execution
- Authentication bypass
- Data breach potential

**High** (< 7 days)
- Privilege escalation
- SQL injection
- XSS vulnerabilities

**Medium** (< 30 days)
- CSRF issues
- Information disclosure
- DoS vulnerabilities

**Low** (Next release)
- Configuration issues
- Minor information leaks

## ?? Security Best Practices

### API Keys
- Never commit API keys
- Use environment variables
- Use appsettings.Development.json (gitignored)

### Dependencies
- Keep .NET 10 updated
- Monitor NuGet packages for vulnerabilities
- Use `dotnet list package --vulnerable`

### Code Review
- All PRs reviewed for security
- Automated security scans (future)
- Dependency vulnerability checks

## ?? Security Updates

Security updates will be:
- Released as patch versions
- Documented in CHANGELOG.md
- Announced in release notes

## ?? Credit

Security researchers who responsibly disclose vulnerabilities will be credited (with permission) in:
- SECURITY.md
- Release notes
- CONTRIBUTORS.md

---

**Thank you for helping keep Solo Adventure System secure!** ??
