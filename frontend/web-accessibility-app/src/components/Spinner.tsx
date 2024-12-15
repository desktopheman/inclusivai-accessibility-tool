import React from "react";
import { motion } from "framer-motion";

const Spinner: React.FC = () => {
  return (
    <motion.div
      className="spinner-container"
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      transition={{ duration: 0.5 }}
    >
      <div className="spinner"></div>
    </motion.div>
  );
};

export default Spinner;
