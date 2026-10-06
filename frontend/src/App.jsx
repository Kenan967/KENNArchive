import { Link } from "react-router-dom";

const products = [
  { name: "Archive Oversized Tee", price: "59 AZN" },
  { name: "Essential Black Hoodie", price: "119 AZN" },
  { name: "Signature Relaxed Shirt", price: "89 AZN" }
];

export default function App() {
  return (
    <div className="site">
      <header className="header">
        <Link className="logo" to="/">KENN<span>ARCHIVE</span></Link>
        <nav>
          <a href="#collection">Collection</a>
          <a href="#about">About</a>
          <a href="#contact">Contact</a>
        </nav>
        <div className="header-actions">
          <button aria-label="Search">Search</button>
          <button aria-label="Wishlist">Wishlist</button>
          <button aria-label="Cart">Cart</button>
        </div>
      </header>

      <main>
        <section className="hero">
          <div className="hero-copy">
            <p className="eyebrow">KENN ARCHIVE / 2026</p>
            <h1>Quiet pieces.<br /><em>Strong identity.</em></h1>
            <p className="hero-text">
              A curated clothing archive built around clean silhouettes,
              everyday essentials and considered details.
            </p>
            <a className="primary-button" href="#collection">Explore collection</a>
          </div>
          <div className="hero-panel">
            <span>01 — NEW ARCHIVE</span>
            <strong>FORM<br />FUNCTION<br />IDENTITY</strong>
          </div>
        </section>

        <section id="collection" className="collection">
          <div className="section-heading">
            <div><p className="eyebrow">SELECTED PIECES</p><h2>Current collection</h2></div>
            <a href="#collection">View all</a>
          </div>
          <div className="products">
            {products.map((product, index) => (
              <article className="product" key={product.name}>
                <div className={"product-image image-" + index}>
                  <span>KENN / {String(index + 1).padStart(2, "0")}</span>
                </div>
                <div className="product-info">
                  <h3>{product.name}</h3>
                  <p>{product.price}</p>
                </div>
              </article>
            ))}
          </div>
        </section>

        <section id="about" className="manifesto">
          <p className="eyebrow">THE ARCHIVE</p>
          <h2>Less noise.<br />More character.</h2>
          <p>
            KENN Archive focuses on wearable pieces with a clear visual
            language — simple, structured and made to become part of your
            everyday wardrobe.
          </p>
        </section>
      </main>

      <footer id="contact">
        <span>© 2026 KENN ARCHIVE</span>
        <span>Built as a course project with a real-world product mindset.</span>
      </footer>
    </div>
  );
}
