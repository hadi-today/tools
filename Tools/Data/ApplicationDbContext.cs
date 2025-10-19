using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Tools.Models;
using Tools.Models.Core;
using ProjectTask = Tools.Models.Core.Task;
using System.Collections.Generic;

namespace Tools.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects { get; set; } = null!;

    public DbSet<ProjectMember> ProjectMembers { get; set; } = null!;

    public DbSet<WebsiteFeature> WebsiteFeatures { get; set; } = null!;

    public DbSet<ProjectTask> Tasks { get; set; } = null!;

    public DbSet<TaskComment> TaskComments { get; set; } = null!;

    public DbSet<Invitation> Invitations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>()
            .Property(user => user.HourlyRate)
            .HasDefaultValue(ApplicationUser.DefaultHourlyRate);

        builder.Entity<ProjectMember>()
            .HasKey(member => new { member.ProjectId, member.UserId });

        builder.Entity<ProjectMember>()
            .HasOne(member => member.Project)
            .WithMany(project => project.Members)
            .HasForeignKey(member => member.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ProjectMember>()
            .HasOne(member => member.User)
            .WithMany()
            .HasForeignKey(member => member.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<TaskComment>()
            .HasOne(comment => comment.Task)
            .WithMany(task => task.Comments)
            .HasForeignKey(comment => comment.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<TaskComment>()
            .HasOne(comment => comment.User)
            .WithMany()
            .HasForeignKey(comment => comment.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<WebsiteFeature>()
            .HasOne(feature => feature.ParentFeature)
            .WithMany(feature => feature.ChildFeatures)
            .HasForeignKey(feature => feature.ParentFeatureId)
            .OnDelete(DeleteBehavior.Restrict);

        var websiteFeatures = new List<WebsiteFeature>
        {
            new() { Id = 1, Title = "Project Foundation & Scope", Description = "Establish the project's foundational characteristics and strategic scope." },
            new() { Id = 2, Title = "Core Website Blueprints", Description = "Select the core blueprint that best matches the project's goals." },
            new() { Id = 3, Title = "Common Modules & Add-ons", Description = "Enhance the project with reusable modules and add-on capabilities." },
            new() { Id = 4, Title = "Technical & Foundational Features", Description = "Ensure non-functional requirements and technical safeguards are in place." },

            new() { Id = 5, Title = "Project Licensing & Type", Description = "Define the overall licensing model for the project.", ParentFeatureId = 1 },
            new() { Id = 6, Title = "Proprietary / Commercial Project", Description = "Closed-source solution released under a commercial license.", ParentFeatureId = 5 },
            new() { Id = 7, Title = "Open-Source Project", Description = "Public repository with community collaboration under licenses like MIT or GPL.", ParentFeatureId = 5 },

            new() { Id = 8, Title = "Target Platform", Description = "Determine who will access and use the final solution.", ParentFeatureId = 1 },
            new() { Id = 9, Title = "Public-Facing Website", Description = "Accessible to anyone with an internet connection.", ParentFeatureId = 8 },
            new() { Id = 10, Title = "Internal Business Application / Intranet", Description = "Restricted to company employees or internal stakeholders.", ParentFeatureId = 8 },
            new() { Id = 11, Title = "Software as a Service (SaaS) Platform", Description = "Subscription-based product designed for multi-tenant usage.", ParentFeatureId = 8 },

            new() { Id = 12, Title = "Corporate / Business Website", Description = "Professional online presence for established organisations.", ParentFeatureId = 2 },
            new() { Id = 13, Title = "Professional Homepage", Description = "Hero section with value proposition and clear calls-to-action.", ParentFeatureId = 12 },
            new() { Id = 14, Title = "About Us Page", Description = "Company history, mission and vision messaging.", ParentFeatureId = 12 },
            new() { Id = 15, Title = "Services / Products Showcase Page", Description = "Detailed listing of key offerings and differentiators.", ParentFeatureId = 12 },
            new() { Id = 16, Title = "Team Members Page", Description = "Profiles and biographies of key personnel.", ParentFeatureId = 12 },
            new() { Id = 17, Title = "Customer Testimonials Section", Description = "Quotes and feedback from satisfied customers.", ParentFeatureId = 12 },
            new() { Id = 18, Title = "Contact Page", Description = "Contact form, map and essential contact details.", ParentFeatureId = 12 },

            new() { Id = 19, Title = "E-commerce Platform", Description = "Commerce-ready foundation with catalogue and checkout flows.", ParentFeatureId = 2 },
            new() { Id = 20, Title = "Product Catalog System", Description = "Structured catalogue management with rich product data.", ParentFeatureId = 19 },
            new() { Id = 21, Title = "Product Listing Pages", Description = "Grid or list views with filtering and sorting options.", ParentFeatureId = 20 },
            new() { Id = 22, Title = "Product Detail Pages", Description = "Image gallery, descriptions, specifications and pricing.", ParentFeatureId = 20 },
            new() { Id = 23, Title = "Product Categories & Tags", Description = "Hierarchical taxonomy for browsing and discovery.", ParentFeatureId = 20 },
            new() { Id = 24, Title = "Inventory Management", Description = "Track stock levels and manage product availability.", ParentFeatureId = 20 },

            new() { Id = 25, Title = "Shopping Cart & Checkout", Description = "Comprehensive purchase workflow for customers.", ParentFeatureId = 19 },
            new() { Id = 26, Title = "Add to Cart Functionality", Description = "Seamless product selection and cart management.", ParentFeatureId = 25 },
            new() { Id = 27, Title = "Multi-step Checkout Process", Description = "Shipping, billing and payment captured across guided steps.", ParentFeatureId = 25 },
            new() { Id = 28, Title = "Guest Checkout Option", Description = "Allow purchases without mandatory account creation.", ParentFeatureId = 25 },

            new() { Id = 29, Title = "Payment Gateway Integration", Description = "Secure payment processing with third-party providers.", ParentFeatureId = 19 },
            new() { Id = 30, Title = "Credit Card Processing (e.g., Stripe)", Description = "Integrate leading credit card payment services.", ParentFeatureId = 29 },
            new() { Id = 31, Title = "PayPal Integration", Description = "Offer PayPal as an alternative payment method.", ParentFeatureId = 29 },

            new() { Id = 32, Title = "User Account Management", Description = "Customer account area for orders and profile settings.", ParentFeatureId = 19 },
            new() { Id = 33, Title = "Order History & Status Tracking", Description = "Customers can review previous orders and live statuses.", ParentFeatureId = 32 },
            new() { Id = 34, Title = "Saved Shipping & Billing Addresses", Description = "Store frequently used delivery and payment information.", ParentFeatureId = 32 },

            new() { Id = 35, Title = "Advanced E-commerce Features", Description = "Enhancements that drive engagement and repeat sales.", ParentFeatureId = 19 },
            new() { Id = 36, Title = "Product Reviews & Ratings System", Description = "Enable customers to leave qualitative feedback.", ParentFeatureId = 35 },
            new() { Id = 37, Title = "Discount Codes & Promotions Engine", Description = "Configurable promotions and coupon management.", ParentFeatureId = 35 },
            new() { Id = 38, Title = "Wishlist / Favorites Functionality", Description = "Let customers save items for later consideration.", ParentFeatureId = 35 },

            new() { Id = 39, Title = "Blog / Content Platform", Description = "Robust publishing toolkit for editorial teams.", ParentFeatureId = 2 },
            new() { Id = 40, Title = "Article / Post Management", Description = "Create, edit and manage long-form content.", ParentFeatureId = 39 },
            new() { Id = 41, Title = "Rich Text Editor (WYSIWYG)", Description = "Full-featured editing experience for content creators.", ParentFeatureId = 40 },
            new() { Id = 42, Title = "Scheduled Publishing", Description = "Plan content releases ahead of time.", ParentFeatureId = 40 },
            new() { Id = 43, Title = "Content Organization", Description = "Structure and categorise articles for discoverability.", ParentFeatureId = 39 },
            new() { Id = 44, Title = "Categories & Tags System", Description = "Taxonomy-based organisation for browsing content.", ParentFeatureId = 43 },
            new() { Id = 45, Title = "Search Functionality", Description = "Allow readers to quickly locate relevant content.", ParentFeatureId = 43 },
            new() { Id = 46, Title = "Reader Engagement", Description = "Interactive tools that keep readers connected.", ParentFeatureId = 39 },
            new() { Id = 47, Title = "Comment System (with moderation)", Description = "Threaded discussions with moderation controls.", ParentFeatureId = 46 },
            new() { Id = 48, Title = "Social Sharing Buttons", Description = "Easy sharing to major social platforms.", ParentFeatureId = 46 },
            new() { Id = 49, Title = "Author Profiles & Management", Description = "Dedicated space for contributor biographies.", ParentFeatureId = 39 },

            new() { Id = 50, Title = "Personal Portfolio / Showcase", Description = "Highlight individual projects and personal expertise.", ParentFeatureId = 2 },
            new() { Id = 51, Title = "Project Gallery", Description = "Visual gallery with imagery, video and summaries.", ParentFeatureId = 50 },
            new() { Id = 52, Title = "Detailed Project Pages", Description = "Deep dives into selected projects or case studies.", ParentFeatureId = 50 },
            new() { Id = 53, Title = "Bio / Resume Page", Description = "Comprehensive biography and resume style information.", ParentFeatureId = 50 },
            new() { Id = 54, Title = "Skills & Expertise Section", Description = "Summarise competencies and technical proficiencies.", ParentFeatureId = 50 },
            new() { Id = 55, Title = "Contact Form for Inquiries", Description = "Allow visitors to reach out for collaborations or projects.", ParentFeatureId = 50 },

            new() { Id = 56, Title = "User Authentication System", Description = "Secure account lifecycle and authentication flows.", ParentFeatureId = 3 },
            new() { Id = 57, Title = "Standard Registration", Description = "Email and password-based user registration.", ParentFeatureId = 56 },
            new() { Id = 58, Title = "Social Logins", Description = "Support Google, Facebook, GitHub and similar OAuth providers.", ParentFeatureId = 56 },
            new() { Id = 59, Title = "Two-Factor Authentication (2FA)", Description = "Add multi-step verification for increased security.", ParentFeatureId = 56 },
            new() { Id = 60, Title = "Password Reset Functionality", Description = "Self-service password reset workflow.", ParentFeatureId = 56 },
            new() { Id = 61, Title = "User Roles & Permissions System", Description = "Role-based access control for different audiences.", ParentFeatureId = 56 },

            new() { Id = 62, Title = "Internationalization (i18n)", Description = "Support multilingual and multi-region experiences.", ParentFeatureId = 3 },
            new() { Id = 63, Title = "Multi-Language Support", Description = "Toggleable interface supporting multiple languages.", ParentFeatureId = 62 },
            new() { Id = 64, Title = "Multi-Currency Support", Description = "Display pricing in the visitor's preferred currency.", ParentFeatureId = 62 },
            new() { Id = 65, Title = "Region-Specific Content Delivery", Description = "Serve tailored content based on visitor location.", ParentFeatureId = 62 },

            new() { Id = 66, Title = "Content & Media Management", Description = "Back-office tools for managing static and media assets.", ParentFeatureId = 3 },
            new() { Id = 67, Title = "Media Library", Description = "Centralised storage for images, videos and documents.", ParentFeatureId = 66 },
            new() { Id = 68, Title = "Static Page Management", Description = "Manage evergreen pages such as Privacy Policy or Terms.", ParentFeatureId = 66 },
            new() { Id = 69, Title = "CMS Interface", Description = "Friendly content management interface for non-technical users.", ParentFeatureId = 66 },

            new() { Id = 70, Title = "Interactive Features", Description = "Engagement-focused interactive components.", ParentFeatureId = 3 },
            new() { Id = 71, Title = "Newsletter Subscription Form", Description = "Collect subscribers and integrate with tools like Mailchimp.", ParentFeatureId = 70 },
            new() { Id = 72, Title = "Booking / Appointment Scheduling System", Description = "Allow users to schedule appointments or demos.", ParentFeatureId = 70 },
            new() { Id = 73, Title = "Live Chat Integration", Description = "Connect live chat tools such as Intercom or Crisp.", ParentFeatureId = 70 },
            new() { Id = 74, Title = "Forums / Community Discussion Board", Description = "Facilitate asynchronous community conversations.", ParentFeatureId = 70 },

            new() { Id = 75, Title = "SEO & Analytics", Description = "Optimise visibility and capture actionable insights.", ParentFeatureId = 4 },
            new() { Id = 76, Title = "On-Page SEO Toolkit", Description = "Manage meta data, structured URLs and SEO basics.", ParentFeatureId = 75 },
            new() { Id = 77, Title = "Automated XML Sitemap Generation", Description = "Keep search engines up to date with site structure.", ParentFeatureId = 75 },
            new() { Id = 78, Title = "Integration with Google Analytics & GTM", Description = "Seamless analytics and tag management connections.", ParentFeatureId = 75 },
            new() { Id = 79, Title = "Structured Data (Schema.org) Markup", Description = "Add schema markup for enhanced search snippets.", ParentFeatureId = 75 },

            new() { Id = 80, Title = "Security & Compliance", Description = "Protect the platform and meet regulatory requirements.", ParentFeatureId = 4 },
            new() { Id = 81, Title = "Enforce SSL (HTTPS)", Description = "Force encrypted connections across the application.", ParentFeatureId = 80 },
            new() { Id = 82, Title = "Protection against OWASP Top 10", Description = "Mitigate the most common web application vulnerabilities.", ParentFeatureId = 80 },
            new() { Id = 83, Title = "GDPR / CCPA Compliance Tools", Description = "Offer consent management and data privacy controls.", ParentFeatureId = 80 },
            new() { Id = 84, Title = "Automated Backups", Description = "Routine backups to protect against data loss.", ParentFeatureId = 80 },

            new() { Id = 85, Title = "Performance & Scalability", Description = "Ensure the platform is fast and future ready.", ParentFeatureId = 4 },
            new() { Id = 86, Title = "Responsive Design", Description = "Optimised layouts for mobile, tablet and desktop.", ParentFeatureId = 85 },
            new() { Id = 87, Title = "Image Optimization & Lazy Loading", Description = "Deliver visual assets efficiently and responsively.", ParentFeatureId = 85 },
            new() { Id = 88, Title = "Content Delivery Network (CDN) Integration", Description = "Distribute content across global edge locations.", ParentFeatureId = 85 },
            new() { Id = 89, Title = "Advanced Caching Strategy", Description = "Client and server-side caching for consistently fast loads.", ParentFeatureId = 85 }
        };

        builder.Entity<WebsiteFeature>().HasData(websiteFeatures);
    }
}
